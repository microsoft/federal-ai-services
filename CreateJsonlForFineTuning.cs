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

namespace Microsoft.Function
{
    public static class CreateJsonlForFineTuning
    {
        class WebSource
        {
            public string webpageUrl { get; set; }
        }

        [FunctionName("CreateJsonlForFineTuning")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "Format data for fine tuning" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(WebSource), Description = "Webpage with data", Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string url = data?.webpageUrl;

            if (string.IsNullOrEmpty(url))
            {
                return new BadRequestObjectResult("Please pass a URL in the request body");
            }

            string keyFromEnvironment = Environment.GetEnvironmentVariable("DOCUMENT_INTELLIGENCE_API_KEY");
            string endpointFromEnvironment = Environment.GetEnvironmentVariable("DOCUMENT_INTELLIGENCE_API_ENDPOINT");

            AzureKeyCredential credential = new AzureKeyCredential(keyFromEnvironment);
            DocumentIntelligenceClient client = new DocumentIntelligenceClient(new Uri(endpointFromEnvironment), credential);

            Uri fileUri = new Uri(url);

            AnalyzeDocumentContent content = new AnalyzeDocumentContent()
            {
                UrlSource = fileUri
            };

            Operation<AnalyzeResult> operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-layout", content);

            AnalyzeResult result = operation.Value;

            string combinedContent = string.Join(" ", result.Paragraphs
            .Select(paragraph => paragraph.Content));

            var jsonlData = new[]
            {
                new
                {
                    messages = new[]
                    {
                        new { role = "system", content = "You are an expert in NASA Apollo missions" },
                        new { role = "user", content = "Please provide information from the documents." },
                        new { role = "assistant", content = combinedContent }
                    }
                }
            };

            string jsonlOutput = string.Join("\n", jsonlData.Select(JsonConvert.SerializeObject));

            Console.WriteLine(jsonlOutput);

            return new OkObjectResult(jsonlOutput);
        }
    }
}