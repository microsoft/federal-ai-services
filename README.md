# Federal AI Services
Welcome to the Federal AI Services Demonstration repository! This project showcases how to implement FedRAMP High AI solutions using Azure AI Services. 

Overview:
This repository serves as a demonstration platform for building secure and compliant AI applications. It provides examples and best practices for implementing AI solutions that adhere to security best practices.

Features:
* FedRAMP High: Provides guidelines and examples for achieving FedRAMP High compliance.
* Azure OpenAI Integration: Utilizes Azure OpenAI with GPT-4 chat completions as the first, upcoming endpoint. 
* Security Best Practices: Includes security measures and reporting processes to ensure the safety and integrity of the AI applications.

Completed: 
* Scaffold Azure Function Project
* Implement Azure OpenAI Endpoint Boilerplate

WIP: 
* Add content safety to Azure OpenAI Endpoint

TO DO: 
* Add input sanitization to Azure OpenAI Endpoint
* Implement a RAG Endpoint with data governance
* More AI Integrations!

Running locally: 
* Prerequisites:
  *  [VS Code](https://code.visualstudio.com/download)
  *  An [Azure account](https://azure.microsoft.com/free/?ref=microsoft.com&utm_source=microsoft.com&utm_medium=docs&utm_campaign=visualstudio) with an active subscription.
  *  An [Azure OpenAI Resource](https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/create-resource?pivots=web-portal)
  *  [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
  *  [C# extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp) for Visual Studio Code.
  *  [Azure Functions extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp) for Visual Studio Code.
*  Clone the repository
*  Create a local.settings.json file that contains the values that align to the properties in the example.local.settings.json
*  Navigate to the root of the repository from your terminal
*  Run `func host start` from your terminal

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft 
trademarks or logos is subject to and must follow 
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
