#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0001

// Retrieve API keys from environment variables
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.TextToAudio;
using Microsoft.SemanticKernel.TextToImage;
using Spectre.Console;
using System.Text.RegularExpressions;

var openAIKey = Environment.GetEnvironmentVariable("SKCourseOpenAIKey");
var azureRegion = Environment.GetEnvironmentVariable("SKCourseAzureRegion");
var azureKey = Environment.GetEnvironmentVariable("SKCourseAzureKey");

// Initialize kernels
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


#region Blog Post Generation

string topic = AnsiConsole.Ask<string>("What is the topic of the blog post?");

string contentPrompt = $@"Generate a detailed blog post about '{topic}'. 
Include an introduction, several main paragraphs, code snippets if needed and a conclusion.
Separate each section with a heading.
It's mandatory that you use the following Gutenberg blocks for a wordpress site:
Heading
<!-- wp:heading {{""level"":1,""fontSize"":""level-6""}} --> <h1 class=""wp-block-heading has-level-6-font-size"">Say Hello to Gutenberg, the WordPress Editor</h1> <!-- /wp:heading --> *Modify `""level"":1` and `""fontSize"":""level-6""` according to the desired heading level and size.*

Paragraph
<!-- wp:paragraph --> <p>This is a sample paragraph in Gutenberg. You can add more text here to show how a paragraph block would look.</p> <!-- /wp:paragraph -->

Unordered List
<!-- wp:list --> <ul> <li>List item 1</li> <li>List item 2</li> <li>List item 3</li> </ul> <!-- /wp:list -->

Ordered List
<!-- wp:list {{""ordered"":true}} --> <ol> <li>First item</li> <li>Second item</li> <li>Third item</li> </ol> <!-- /wp:list -->

Quote
<!-- wp:quote --> <blockquote class=""wp-block-quote""> <p>""Experience is simply the name we give our mistakes."" – Oscar Wilde</p> <cite>Oscar Wilde</cite> </blockquote> <!-- /wp:quote -->

Code
<!-- wp:code --> <pre class=""wp-block-code""><code>function helloWorld() {{ console.log(""Hello, world!""); }}</code></pre> <!-- /wp:code -->

Bold Text and Links Within a Paragraph
<!-- wp:paragraph --> <p>This is a <strong>bold text</strong> with a <a href=""https://example.com"">link</a> example.</p> <!-- /wp:paragraph -->

Italic and Underlined Text
<!-- wp:paragraph --> <p>This is an <em>italic text</em> and <u>underlined</u> within a paragraph.</p> <!-- /wp:paragraph -->
";

AnsiConsole.MarkupLine("[bold yellow]STARTING BLOG POST GENERATION[/]");

string blogContent = string.Empty;


await AnsiConsole.Status()
    .Spinner(Spinner.Known.Default)
    .StartAsync("Generating blog post content...", async ctx =>
    {
        var result = await azureKernel.InvokePromptAsync(contentPrompt);
        blogContent = result.ToString();
    });

AnsiConsole.MarkupLine("[green]BLOG POST CONTENT GENERATED[/]");

AnsiConsole.Write(new Text(blogContent));


#endregion


#region Featured Image


var imageService = azureKernel.GetRequiredService<ITextToImageService>();

string imagePrompt = 
        $@"Generate a featured image representing '{topic}' in a professional blog style ";

var imageOptions = new OpenAITextToImageExecutionSettings
{
    Quality = "high",
    Size = (1792, 1024),
    Style = "vivid"
};

AnsiConsole.MarkupLine("\n[bold yellow]STARTING FEATURED IMAGE GENERATION[/]\n");

string featuredImage = string.Empty;

await AnsiConsole.Status()
    .Spinner(Spinner.Known.Default)
    .StartAsync("Generating featured image...", async ctx =>
    {
        var result = await imageService.GetImageContentsAsync(imagePrompt, imageOptions);
        featuredImage = result[0].Uri!.ToString();
    });

AnsiConsole.MarkupLine("[green]FEATURED IMAGE GENERATED[/]");
AnsiConsole.MarkupLine($"[underline blue]Featured Image URL: {featuredImage}[/]");


#endregion


#region Audio File


string audioSavePath = "D:\\tests\\blog_post_audio.mp3";

var cleanContent = CleanWPContent(blogContent);

var audioService = azureKernel.GetRequiredService<ITextToAudioService>();

var audioOptions = new OpenAITextToAudioExecutionSettings
{
    Voice = "alloy",
    Speed = 1.0f,
    ResponseFormat = "mp3"
};


AnsiConsole.MarkupLine("\n[bold yellow]STARTING AUDIO FILE GENERATION[/]\n");

await AnsiConsole.Status()
    .Spinner(Spinner.Known.Default)
    .StartAsync("Generating audio file...", async ctx =>
    {
        var audioContent = await audioService
            .GetAudioContentAsync(cleanContent, audioOptions);
        await File.WriteAllBytesAsync(audioSavePath, audioContent.Data!.Value.ToArray());
    });

AnsiConsole.MarkupLine($"[green]AUDIO FILE GENERATED AT: {audioSavePath}[/]");


static string CleanWPContent(string input)
{
    if (string.IsNullOrEmpty(input))
    {
        return input;
    }

    // Remove WordPress block comments (start and end tags)
    string withoutWpComments = Regex.Replace(input, @"<!--\s*/?wp:.*?-->", string.Empty, RegexOptions.Singleline);

    // Remove all HTML tags
    string cleanText = Regex.Replace(withoutWpComments, @"<[^>]+>", string.Empty);

    // Decode HTML entities and trim extra whitespace
    return System.Net.WebUtility.HtmlDecode(cleanText).Trim();
}


#endregion