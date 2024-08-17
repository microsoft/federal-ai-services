using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using OpenAI.Chat;


namespace Microsoft.Function
{
    public class AzureOpenAICompleteChatWith4o
    {
        private readonly ILogger<AzureOpenAICompleteChatWith4o> _logger;

        public AzureOpenAICompleteChatWith4o(ILogger<AzureOpenAICompleteChatWith4o> log)
        {
            _logger = log;
        }

        class Question 
        {
            public string question { get; set; }

            public string systemMessage {get; set;}
        }

       
        [FunctionName("AzureOpenAICompleteChatWith4o")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "Get Chat Completion with GPT 4o" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(Question), Description = "Question for GPT", Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            string keyFromEnvironment = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
            string endpointFromEnvironment = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
            string modelFromEnvironment = Environment.GetEnvironmentVariable("AZURE_OPENAI_MODEL");

            AzureOpenAIClient azureClient = new(
            new Uri(endpointFromEnvironment),
            new AzureKeyCredential(keyFromEnvironment));
            ChatClient chatClient = azureClient.GetChatClient(modelFromEnvironment);

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string userQuestion = data.question;
            string systemMessage = data.systemMessage;

            ChatCompletion completion = chatClient.CompleteChat(
                [
                new SystemChatMessage(systemMessage),
                new UserChatMessage(userQuestion),
            ]);

            Console.WriteLine($"{completion.Role}: {completion.Content[0].Text}");

            return new OkObjectResult(completion.Content[0].Text);
        }
    }
}

