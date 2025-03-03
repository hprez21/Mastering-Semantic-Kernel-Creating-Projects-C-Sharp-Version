#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0050
#pragma warning disable SKEXP0040

using KernelMediaTool.Plugins;
using KernelMediaTool.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Spectre.Console;

var openAIKey = Environment.GetEnvironmentVariable("SKCourseOpenAIKey");
var azureRegion = Environment.GetEnvironmentVariable("SKCourseAzureRegion");
var azureKey = Environment.GetEnvironmentVariable("SKCourseAzureKey");

var azureBuilder = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion("gpt-4o-mini",
        $"{azureRegion}",
        $"{azureKey}")
    .AddAzureOpenAIAudioToText("whisper", endpoint: $"{azureRegion}", apiKey: $"{azureKey}");


azureBuilder.Services.AddSingleton<FFMPegUtils>();
azureBuilder.Plugins.AddFromType<VideoPlugin>();

azureBuilder.Services.AddSingleton<WhisperTranscriptionService>();
azureBuilder.Plugins.AddFromType<SpeechToTextPlugin>();


var azureKernel = azureBuilder.Build();

var chatCompletionService =
    azureKernel.GetRequiredService<IChatCompletionService>();

OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

ChatHistory chatHistory = new();
string? input = null;

chatHistory.AddAssistantMessage("Hi! How would you like to get started? How about trying to get the transcript of your video by providing the path to the file?");

AnsiConsole.MarkupLine($"[bold green]Assistant > [/][italic]{chatHistory.LastOrDefault()}[/]");

while(true)
{
    AnsiConsole.Markup("\n[Bold blue]User > [/]");
    input = Console.ReadLine();

    if(string.IsNullOrWhiteSpace(input))
    {
        break;
    }
    chatHistory.AddUserMessage(input);

    var chatResult = await
        chatCompletionService.GetChatMessageContentAsync(
            chatHistory,
            openAIPromptExecutionSettings,
            azureKernel);

    chatHistory.AddAssistantMessage(chatResult.ToString());
    AnsiConsole.MarkupLine($"[bold green]Assistant > [/][italic]{chatResult}[/]");
}