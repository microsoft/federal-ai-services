using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
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

        [FunctionName("AzureOpenAICompleteChatWith4o")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            string keyFromEnvironment = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
            string endpointFromEnvironment = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
            string modelFromEnvironment = Environment.GetEnvironmentVariable("AZURE_OPENAI_MODEL");

            AzureOpenAIClient azureClient = new(
            new Uri(endpointFromEnvironment),
            new AzureKeyCredential(keyFromEnvironment));
            ChatClient chatClient = azureClient.GetChatClient(modelFromEnvironment);

            ChatCompletion completion = chatClient.CompleteChat(
                [
                // System messages represent instructions or other guidance about how the assistant should behave
                new SystemChatMessage("You are a helpful assistant that talks like a pirate."),
                // User messages represent user input, whether historical or the most recen tinput
                new UserChatMessage("Hi, can you help me?"),
                // Assistant messages in a request represent conversation history for responses
                new AssistantChatMessage("Arrr! Of course, me hearty! What can I do for ye?"),
                new UserChatMessage("What's the best way to train a parrot?"),
            ]);

            Console.WriteLine($"{completion.Role}: {completion.Content[0].Text}");

            return new OkObjectResult(completion.Content[0].Text);
        }
    }
}

