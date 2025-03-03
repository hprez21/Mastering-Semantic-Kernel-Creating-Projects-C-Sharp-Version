using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

var openAIKey = Environment.GetEnvironmentVariable("SKCourseOpenAIKey");
var azureRegion = Environment.GetEnvironmentVariable("SKCourseAzureRegion");
var azureKey = Environment.GetEnvironmentVariable("SKCourseAzureKey");

Kernel openAIKernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion("gpt-4o-mini-2024-07-18", $"{openAIKey}")
    .Build();

Kernel azureKernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion("gpt-4o-mini",
        $"{azureRegion}",
        $"{azureKey}")
    .Build();

var executionSettings = new OpenAIPromptExecutionSettings
{
    MaxTokens = 4000,
    Temperature = 0.7,
};


#region Simple Chat

//string prompt = "What is the capital of France?";

//var chatCompletionService = openAIKernel.GetRequiredService<IChatCompletionService>();


//var response =
//    await chatCompletionService.GetChatMessageContentAsync(prompt, executionSettings);

//Console.WriteLine(response);


#endregion

#region Chat History

//string prompt = "What is my name?";

//var chatCompletionService = openAIKernel.GetRequiredService<IChatCompletionService>();

//ChatHistory chatHistory = [];

//chatHistory.AddSystemMessage("You are a helpful assistant");
//chatHistory.AddAssistantMessage("Welcome to the system! Please tell me your name to get started.");
//chatHistory.AddUserMessage("My name is Héctor");
//chatHistory.AddAssistantMessage("Nice to meet you, Héctor! How can I help you today?");
//chatHistory.AddUserMessage(prompt);


//var response =
//    await chatCompletionService.GetChatMessageContentAsync(chatHistory, executionSettings);

//Console.WriteLine(response);


#endregion

#region Multi-modal Chat

var chatCompletionService = openAIKernel.GetRequiredService<IChatCompletionService>();

ChatHistory chatHistory = new ChatHistory("You are an expert at obtaining object tags from an image");

//chatHistory.AddUserMessage([

//        new TextContent("Give me the tags of the image"),
//        new ImageContent(new Uri("https://www.telegraph.co.uk/content/dam/Travel/Destinations/North%20America/USA/New%20York/newyork-skyline-GettyImages-1347979016.jpg"))

//    ]);

var imageBytes = File.ReadAllBytes("G:\\images\\nature.jpg");


chatHistory.AddUserMessage([

        new TextContent("Give me the tags of the image"),
        new ImageContent(imageBytes, "image/jpeg")

    ]);

var response =
    await chatCompletionService.GetChatMessageContentAsync(chatHistory, executionSettings);

Console.WriteLine(response);

#endregion