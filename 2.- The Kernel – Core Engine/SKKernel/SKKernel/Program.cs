using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.TextToAudio;
using Microsoft.SemanticKernel.TextToImage;
using System.Drawing;

#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0001

var openAIKey = Environment.GetEnvironmentVariable("SKCourseOpenAIKey");
var azureRegion = Environment.GetEnvironmentVariable("SKCourseAzureRegion");
var azureKey = Environment.GetEnvironmentVariable("SKCourseAzureKey");

Kernel openAIKernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion("gpt-4o-mini-2024-07-18", $"{openAIKey}")
    .AddOpenAITextToImage($"{openAIKey}")
    .AddOpenAITextToAudio("tts-1", $"{openAIKey}")
    .AddOpenAIAudioToText("whisper-1", $"{openAIKey}")
    .Build();



Kernel azureKernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion("gpt-4o-mini",
        $"{azureRegion}",
        $"{azureKey}")
    .AddAzureOpenAITextToImage("dall-e-3", endpoint: $"{azureRegion}", apiKey: $"{azureKey}")
    .AddAzureOpenAITextToAudio("tts-hd", endpoint: $"{azureRegion}", apiKey: $"{azureKey}")
    .AddAzureOpenAIAudioToText("whisper", endpoint: $"{azureRegion}", apiKey: $"{azureKey}")
    .Build();

#region Chat Completion

var options = new OpenAIPromptExecutionSettings
{
    MaxTokens = 50,
    Temperature = 0.9
};


var prompt = "Write a short poem about Semantic Kernel";

var result = azureKernel.InvokePromptAsync(prompt, new KernelArguments(options));

Console.WriteLine(result.Result);

#endregion

#region Chat Completion Streaming

//var prompt = "Write a large poem about Semantic Kernel";

//string fullMessage = string.Empty;

//await foreach(var chatUpdate in azureKernel.InvokePromptStreamingAsync<StreamingChatMessageContent>(prompt))
//{
//    if(chatUpdate.Content is { Length: > 0})
//    {
//        fullMessage += chatUpdate.Content;
//        Console.Write(chatUpdate.Content);
//    }
//}


#endregion

#region Generating Images

//var service = azureKernel.GetRequiredService<ITextToImageService>();

//var imagePrompt = "A beautiful sunset over the mountains";

//var options = new OpenAITextToImageExecutionSettings
//{
//    Quality = "high",
//    Size = (1024, 1792),
//    Style = "vivid"
//};

//var generatedImages =
//    await service.GetImageContentsAsync(imagePrompt, executionSettings: options);

//Console.WriteLine(generatedImages[0].Uri!.ToString());



#endregion

#region Generating Audio Files

//var textToAudioService = azureKernel.GetRequiredService<ITextToAudioService>();

//var content = "Hello, my name is Semantic Kernel. I am a powerful AI tool that can help you with your projects. I can generate text, images, and audio for you. How can I help you today?";

//OpenAITextToAudioExecutionSettings executionSettings = new OpenAITextToAudioExecutionSettings
//{
//    Voice = "alloy",
//    ResponseFormat = "mp3",
//    Speed = 1.0f
//};

//var audioContent =
//    await textToAudioService.GetAudioContentAsync(content, executionSettings);

//var path = "D:\\tests";
//var audioFilePath = Path.Combine(path, "audio.mp3");
//await File.WriteAllBytesAsync(audioFilePath, audioContent.Data!.Value.ToArray());

//Console.WriteLine("Finished");

#endregion

#region Extracting Text from Audio Files


var audioFileName = "D:\\tests\\audio.mp3";

var audioToTextService = azureKernel.GetRequiredService<IAudioToTextService>();

var executionSettings =
    new OpenAIAudioToTextExecutionSettings
    {
        Language = "en",
        Prompt = "This is a prompt. You should respect punctuation marks",
        ResponseFormat = "json",
        Temperature = 0.3f
    };

var audioFileStream = File.OpenRead(audioFileName);
var audioFileBinaryData = await BinaryData.FromStreamAsync(audioFileStream);
AudioContent audioContent = new(audioFileBinaryData, mimeType: null);

var textContent = await audioToTextService.GetTextContentAsync(audioContent, executionSettings);

Console.WriteLine(textContent.Text);


#endregion