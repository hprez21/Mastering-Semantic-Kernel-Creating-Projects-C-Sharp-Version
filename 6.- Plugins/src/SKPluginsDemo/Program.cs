#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0050
#pragma warning disable SKEXP0040

using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using SharpYaml;
using SKPluginsDemo.Models;
using SKPluginsDemo.Plugins;
using SKPluginsDemo.Services;

var openAIKey = Environment.GetEnvironmentVariable("SKCourseOpenAIKey");
var azureRegion = Environment.GetEnvironmentVariable("SKCourseAzureRegion");
var azureKey = Environment.GetEnvironmentVariable("SKCourseAzureKey");

var openAIBuilder = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion("gpt-4o-mini-2024-07-18", $"{openAIKey}");

var openAIKernel = openAIBuilder.Build();

var azureBuilder = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion("gpt-4o-mini",
        $"{azureRegion}",
        $"{azureKey}");

//azureBuilder.Services.AddSingleton<IFileService, FileService>();
//azureBuilder.Plugins.AddFromType<FilePlugin>();

azureBuilder.Plugins.AddFromType<SystemInfoPlugin>();

azureBuilder.Plugins.AddFromObject(new FilePlugin(new FileService()));

var speakers = new List<SpeakerModel>
{
    new SpeakerModel { Id = 1, Name = "Laptop", BatteryLevel = 100, VolumeLevel = 50 },
    new SpeakerModel { Id = 2, Name = "Car", BatteryLevel = 80, VolumeLevel = 60 },
    new SpeakerModel { Id = 3, Name = "TV", BatteryLevel = 90, VolumeLevel = 70 }
};

azureBuilder.Plugins.AddFromObject(new SpeakerPlugin(speakers));

var azureKernel = azureBuilder.Build();

//KernelFunction memoryFunction =
//   azureKernel.Plugins.GetFunction("SystemInfoPlugin", "get_memory_ram");



#region Creating Kernel Functions

KernelFunction timeFunction =
    KernelFunctionFactory.CreateFromMethod(() =>
        DateTime.Now.ToShortTimeString(), "get_current_time", "Gets the current time");

KernelFunction poemFunction =
    KernelFunctionFactory
    .CreateFromPrompt("Write a poem about Semantic Kernel", functionName: "write_poem",
            description: "Writes a poem about Semantic Kernel");

//var currentTime = await azureKernel.InvokeAsync(poemFunction);

#endregion

#region Creating your first native plugins
KernelPlugin systemInfoPlugin =
    KernelPluginFactory.CreateFromType<SystemInfoPlugin>();


//var systemInfo = await azureKernel
//    .InvokeAsync(systemInfoPlugin.Where(x => x.Name == "get_top_memory_processes").FirstOrDefault(),
//            new KernelArguments() { { "processes", 5 } });

#endregion

#region Using built-in plugins

KernelPlugin conversationPlugin =
    KernelPluginFactory.CreateFromType<ConversationSummaryPlugin>();
//var filePlugin = KernelPluginFactory.CreateFromType<FileIOPlugin>();
//var httpPlugin = KernelPluginFactory.CreateFromType<HttpPlugin>();
//var mathPlugin = KernelPluginFactory.CreateFromType<MathPlugin>();
//var timePlugin = KernelPluginFactory.CreateFromType<TimePlugin>();
//var textPlugin = KernelPluginFactory.CreateFromType<TextPlugin>();
//var waitPlugin = KernelPluginFactory.CreateFromType<WaitPlugin>();


//var summary = await azureKernel
//    .InvokeAsync(conversationPlugin.Where(x => x.Name == "SummarizeConversation").FirstOrDefault(),
//    new() { { "input", "Semantic Kernel is a robust open-source framework designed to integrate artificial intelligence (AI) with application development seamlessly. It leverages large language models (LLMs) such as GPT or similar technologies, enabling developers to build applications that can comprehend, generate, and reason about textual content. The framework facilitates the creation of applications that blend natural language processing capabilities with traditional programming. At its core, Semantic Kernel introduces the concept of \"semantic memory,\" which allows applications to store and retrieve information in a way that mimics human cognition. This feature is instrumental in building intelligent systems capable of performing tasks like context-aware conversations, summarization, recommendation, and more.\r\n\r\nIn addition to its powerful AI capabilities, Semantic Kernel provides extensibility and flexibility to adapt to a wide range of use cases. Developers can design and manage workflows that dynamically adapt based on user input or real-time data, significantly enhancing user experience. By integrating seamlessly with existing tools and libraries, Semantic Kernel simplifies the AI development process, empowering teams to create solutions without requiring deep expertise in machine learning. Whether for creating advanced chatbots, automating content generation, or enhancing decision-making systems, Semantic Kernel represents a significant step forward in leveraging the potential of AI in software development" } });


//var writeFile = await azureKernel.InvokeAsync(filePlugin.Where(x => x.Name == "Write").FirstOrDefault(),
//                    new() { { "path", "G:\\sk\\test.txt" }, { "content", "This is a demo" } });

//var read = await azureKernel.InvokeAsync(filePlugin.Where(x => x.Name == "Read").FirstOrDefault(),
//                    new() { { "path", "G:\\sk\\test.txt" } });

//var httpGet = await azureKernel.InvokeAsync(httpPlugin.Where(x => x.Name == "Get").FirstOrDefault(),
//                    new() { { "uri", "https://jsonplaceholder.typicode.com/posts/1" } });

//var add = await azureKernel.InvokeAsync(mathPlugin.Where(x => x.Name == "Add").FirstOrDefault(),
//                    new() { { "value", 10.0 }, { "amount", 20.0 } });

//var concat = await azureKernel.InvokeAsync(textPlugin.Where(x => x.Name == "Concat").FirstOrDefault(),
//                    new() { { "input", "Hello " }, { "input2", "World" } });

//var wait = await azureKernel.InvokeAsync(waitPlugin.Where(x => x.Name == "Seconds").FirstOrDefault(),
//                    new() { { "seconds", 5.0m } });

//var time = await azureKernel.InvokeAsync(timePlugin.Where(x => x.Name == "Time").FirstOrDefault(),
//                    new() { });

#endregion



azureKernel.Plugins.AddFromFunctions("MyPlugins", [timeFunction, poemFunction]);



//await azureKernel.ImportPluginFromOpenApiAsync(
//    pluginName: "fakeRest",
//    uri: new Uri("https://fakerestapi.azurewebsites.net/swagger/v1/swagger.json"),
//    executionParameters: new OpenApiFunctionExecutionParameters()
//    {
//        EnablePayloadNamespacing = true
//    });

//KernelPlugin plugin =
//    await OpenApiKernelPluginFactory.CreateFromOpenApiAsync(
//        "fakeRest",
//        new Uri("https://fakerestapi.azurewebsites.net/swagger/v1/swagger.json"),
//        new OpenApiFunctionExecutionParameters()
//        {
//            EnablePayloadNamespacing = true
//        });

using HttpClient client = new HttpClient();
string url = "https://fakerestapi.azurewebsites.net/swagger/v1/swagger.json";
var stream = await client.GetStreamAsync(url);

OpenApiDocumentParser parser = new();

RestApiSpecification specification = await parser.ParseAsync(stream);

RestApiOperation operation = 
    specification.Operations.Single(o => o.Path == "/api/v1/Books");

RestApiParameter idPathParameter = 
    operation.Parameters
    .Single(p => p.Location == RestApiParameterLocation.Path && p.Name == "id");

idPathParameter.ArgumentName = "bookId";

azureKernel.ImportPluginFromOpenApi(pluginName: "books", 
    specification: specification);

//azureKernel.Plugins.Add(plugin);

var chatCompletionService = azureKernel.GetRequiredService<IChatCompletionService>();

OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

ChatHistory chatHistory = [];
string? input = null;

chatHistory.AddUserMessage("What time is it?");

//while (true)
//{
//    ChatMessageContent result =
//        await chatCompletionService.GetChatMessageContentAsync(chatHistory,
//            openAIPromptExecutionSettings, azureKernel);
//    if (result.Content is not null)
//    {
//        Console.Write(result.Content);
//        break;
//    }
//    chatHistory.Add(result);
//    IEnumerable<FunctionCallContent> functionCalls =
//        FunctionCallContent.GetFunctionCalls(result);
//    if (!functionCalls.Any())
//    {
//        break;
//    }
//    foreach (FunctionCallContent functionCall in functionCalls)
//    {
//        try
//        {
//            FunctionResultContent resultContent =
//                await functionCall.InvokeAsync(azureKernel);

//            chatHistory.Add(resultContent.ToChatMessage());
//        }
//        catch (Exception ex)
//        {
//            chatHistory
//                .Add(new FunctionResultContent(functionCall, ex).ToChatMessage());
//        }
//    }
//}


while (true)
{
    Console.Write("\nUser > ");
    input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input))
    {
        break;
    }
    chatHistory.AddUserMessage(input);
    var chatResult = await
        chatCompletionService.GetChatMessageContentAsync(chatHistory,
        openAIPromptExecutionSettings, azureKernel);
    chatHistory.AddAssistantMessage(chatResult.ToString());
    Console.WriteLine($"Assistant > {chatResult}");
}


//Console.WriteLine(httpGet);