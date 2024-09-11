using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using Azure.AI.DocumentIntelligence;
using Azure;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Microsoft.DeepDev;

namespace Microsoft.Function
{
    public static class CreateJsonlForFineTuning
    {
        class WebSource
        {
            public List<string> webpageUrls { get; set; }
        }

        [FunctionName("CreateJsonlForFineTuning")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "Format data for fine tuning" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(WebSource), Description = "Webpages with data", Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            WebSource data = JsonConvert.DeserializeObject<WebSource>(requestBody);

            if (data?.webpageUrls == null || !data.webpageUrls.Any())
            {
                return new BadRequestObjectResult("Please pass an array of URLs in the request body");
            }

            string keyFromEnvironment = Environment.GetEnvironmentVariable("DOCUMENT_INTELLIGENCE_API_KEY");
            string endpointFromEnvironment = Environment.GetEnvironmentVariable("DOCUMENT_INTELLIGENCE_API_ENDPOINT");

            AzureKeyCredential credential = new AzureKeyCredential(keyFromEnvironment);
            DocumentIntelligenceClient client = new DocumentIntelligenceClient(new Uri(endpointFromEnvironment), credential);

            List<dynamic> jsonlData = new List<dynamic>();

            foreach (string url in data.webpageUrls)
            {
                Console.WriteLine($"Analyzing document at URL: {url}");
                Uri fileUri = new Uri(url);

                AnalyzeDocumentContent content = new AnalyzeDocumentContent()
                {
                    UrlSource = fileUri
                };

                Operation<AnalyzeResult> operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-layout", content);

                AnalyzeResult result = operation.Value;

                string combinedContent = string.Join(" ", result.Paragraphs
                    .Select(paragraph => paragraph.Content));

                var jsonObject = new
                {
                    messages = new[]
                    {
                        new { role = "system", content = "You are an expert in NASA Apollo missions, specifically the design and success of heat shields, as well as environmental safety for crew members." },
                        new { role = "user", content = "Provide accurate answers based on Apollo Mission records." },
                        new { role = "assistant", content = combinedContent }
                    }
                };

                // Shorten the JSON object if it exceeds the token limit
                string jsonString = JsonConvert.SerializeObject(jsonObject);
                var tokenizer = await TokenizerBuilder.CreateByModelNameAsync("gpt-4");
                var tokens = tokenizer.Encode(jsonString, Array.Empty<string>());

                Console.WriteLine($"Current Token count: {tokens.Count}");

                while (tokens.Count > 131072)
                {
                    Console.WriteLine($"Token count - Too Many: {tokens.Count}");
                    // Shorten the content to fit within the token limit
                    int excessTokens = tokens.Count - 131072;
                    int charsToRemove = (int)(excessTokens * 0.5); // Adjust this factor as needed
                    combinedContent = combinedContent.Substring(0, combinedContent.Length - charsToRemove);

                    jsonObject = new
                    {
                        messages = new[]
                        {
                            new { role = "system", content = "You are an expert in NASA Apollo missions, specifically the design and success of heat shields, as well as environmental safety for crew members." },
                            new { role = "user", content = "Provide accurate answers based on Apollo Mission records." },
                            new { role = "assistant", content = combinedContent }
                        }
                    };

                    jsonString = JsonConvert.SerializeObject(jsonObject);
                    tokens = tokenizer.Encode(jsonString, Array.Empty<string>());
                }

                jsonlData.Add(jsonObject);
                Console.WriteLine($"Completed document analysis for URL: {url}");
            }

            string jsonlOutput = string.Join("\n", jsonlData.Select(JsonConvert.SerializeObject));

            // Use a sanitized version of the first URL as the file name
            string title = new string(data.webpageUrls.First().Where(char.IsLetterOrDigit).ToArray());
            string localFilePath = Path.Combine(Path.GetTempPath(), $"{title}.jsonl");

            // Write the JSONL output to a local file
            await File.WriteAllTextAsync(localFilePath, jsonlOutput);

            // Upload the JSONL file to Azure OpenAI files system
            string openaiEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT_FINETUNING");
            string openaiApiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY_FINETUNING");
            string apiVersion = "2024-07-01-preview";

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("api-key", openaiApiKey);

            MultipartFormDataContent formDataContent = new MultipartFormDataContent
            {
                { new StringContent("fine-tune"), "purpose" }
            };

            var fileContent = new StreamContent(File.OpenRead(localFilePath));
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            formDataContent.Add(fileContent, "file", $"{title}.jsonl");

            HttpResponseMessage response = await httpClient.PostAsync($"{openaiEndpoint}/openai/files?api-version={apiVersion}", formDataContent);

            if (!response.IsSuccessStatusCode)
            {
                return new BadRequestObjectResult($"Failed to upload file to Azure OpenAI: {response}, status code: {response.StatusCode}, error: {await response.Content.ReadAsStringAsync()}");
            }

            string responseContent = await response.Content.ReadAsStringAsync();

            // Dispose resources
            formDataContent.Dispose();
            httpClient.Dispose();

            // Optionally, delete the local file after upload
            File.Delete(localFilePath);

            Console.WriteLine(jsonlOutput);
            return new OkObjectResult($"File uploaded to Azure OpenAI successfully: {responseContent}");
        }
    }
}