#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0001

using MarkItDownSharp;
using MarkItDownSharp.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.TextToAudio;
using PodcastGenerator;
using Spectre.Console;

var openAIKey = Environment.GetEnvironmentVariable("SKCourseOpenAIKey");

Kernel openAIKernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion("gpt-4o-mini-2024-07-18", $"{openAIKey}")    
    .AddOpenAITextToAudio("tts-1", $"{openAIKey}")    
    .Build();


#region Input Data

AnsiConsole.Write(
    new FigletText("Podcast Generator")
    .Centered()
    .Color(Color.Purple3));

AnsiConsole
    .MarkupLine("[bold yellow]Collecting podcast details...[/]");

var podcastTone =
    AnsiConsole
    .Ask<string>("[green]Enter the podcast tone (e.g. educational, entertaining, informative):[/]");

var hostName =
    AnsiConsole
    .Ask<string>("[green]Enter the host name:[/]");

var guestName =
    AnsiConsole
    .Ask<string>("[green]Enter the guest name:[/]");

var podcastLanguage =
    AnsiConsole
    .Ask<string>("[green]Enter the podcast language:[/]");

var inputDocument =
    AnsiConsole
    .Ask<string>("[green]Enter the input document to be used for generating the script:[/]");

if(!File.Exists(inputDocument))
{
    AnsiConsole
        .MarkupLine("[bold red]The input document does not exist. Please provide a valid file path.[/]");
    return;
}

var inputTable = new Table()
    .Centered()
    .Border(TableBorder.Rounded)
    .AddColumn("[yellow]Parameter[/]")
    .AddColumn("[yellow]Value[/]");

inputTable.AddRow("Tone", podcastTone);
inputTable.AddRow("Host", hostName);
inputTable.AddRow("Guest", guestName);
inputTable.AddRow("Language", podcastLanguage);
inputTable.AddRow("Input Document", inputDocument);

AnsiConsole.Write(new Panel(inputTable)
    .Header("[bold blue]Podcast Details[/]")
    .BorderColor(Color.Blue));

#endregion

#region Markdown conversion

var converter =
    new MarkItDownConverter();

DocumentConverterResult markdown = null;

await AnsiConsole.Status()
    .Spinner(Spinner.Known.Star)
    .Start("Converting document to markdown...", async context =>
    {
        markdown = await converter.ConvertLocalAsync(inputDocument);        
    });

var markdownPanel = new Panel(
    new Markup(
        $"[bold blue]Title:[/]{EscapeMarkup(markdown.Title)}\n\n" +
        $"[bold blue]Content:[/]{EscapeMarkup(markdown.TextContent)}")
        )
    .Header("[bold green]Markdown Conversion[/]")
       .BorderColor(Color.Green)
    .RoundedBorder();
    
AnsiConsole.Write(markdownPanel);


#endregion

#region Utillity Methods
static string EscapeMarkup(string text)
{
    return text
        .Replace("[", "[[")
        .Replace("]", "]]")
        .Replace("<", "\\<")
        .Replace(">", "\\>");
}

#endregion

#region Ideas Generation

string ideasResult =
    string.Empty;

var yamlExecutor = new YamlExecutor();

await AnsiConsole.Status()
    .Spinner(Spinner.Known.Dots12)
    .StartAsync("[bold yellow]Generating Ideas...[/]", async ctx =>
    {
        var ideasArguments = new KernelArguments
        {
            { "documentContent", markdown.TextContent }
        };

        ideasResult = await
            yamlExecutor.ExecuteYaml("GenerateIdeas.yaml", openAIKernel, ideasArguments);
    });

var ideasPanel = new Panel(new Markup($"{EscapeMarkup(ideasResult)}"))
    .Header("[bold yellow]Generated Ideas[/]")
    .BorderColor(Color.Yellow)
    .RoundedBorder();

AnsiConsole.Write(ideasPanel);


#endregion

#region Generating the Podcast Script

string scriptResult = string.Empty;

await AnsiConsole.Status()
    .Spinner(Spinner.Known.Line)
    .StartAsync("[bold green]Generating the podcast script...[/]", async ctx =>
    {
        var scriptArguments = new KernelArguments
        {
            { "documentContent", markdown.TextContent },
            { "hostName", hostName },
            { "guestName", guestName },
            { "podcastTone", podcastTone },
            { "podcastLanguage", podcastLanguage },
            { "firstDraftResult", ideasResult }
        };

        scriptResult =
            await yamlExecutor.ExecuteYaml("GenerateDialog.yaml", openAIKernel, scriptArguments);

    });

var scriptPanel = new Panel(new Markup($"{EscapeMarkup(scriptResult)}"))
    .Header("[bold green]Generated Script[/]")
    .BorderColor(Color.Green)
    .RoundedBorder();

AnsiConsole.Write(scriptPanel);

#endregion

#region Generating the conclusion

string conclusionResult = string.Empty;

await AnsiConsole.Status()
    .Spinner(Spinner.Known.Clock)
    .StartAsync("[bold purple]Generating the conclusion...[/]", async ctx =>
    {
        var conclusionArguments = new KernelArguments
        {
            { "script", scriptResult },
            { "hostName", hostName },
            { "guestName", guestName },
            { "language", podcastLanguage }           
        };
        conclusionResult =
            await yamlExecutor.ExecuteYaml("GenerateConclusion.yaml", openAIKernel, conclusionArguments);
    });

var conclusionPanel = new Panel(new Markup($"{EscapeMarkup(conclusionResult)}"))
    .Header("[bold purple]Generated Conclusion[/]")
    .BorderColor(Color.Purple)
    .RoundedBorder();

AnsiConsole.Write(conclusionPanel);

#endregion

var textToAudioService =
    openAIKernel.GetRequiredService<ITextToAudioService>();

var hostSettings = new OpenAITextToAudioExecutionSettings
{
    Voice = "alloy",
    ResponseFormat = "mp3",
    Speed = 1.0f
};

var guestSettings = new OpenAITextToAudioExecutionSettings
{
    Voice = "nova",
    ResponseFormat = "mp3",
    Speed = 1.0f
};

var finalScript = $"{scriptResult}\n{conclusionResult}";
var lines = finalScript.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
List<byte> finalAudioBytes = new();

var progress = AnsiConsole.Progress()
    .AutoClear(true)
    .HideCompleted(false)
    .Columns(new ProgressColumn[]
    {
        new TaskDescriptionColumn(),
        new ProgressBarColumn(),
        new PercentageColumn(),
        new SpinnerColumn()
    });

await progress.StartAsync(async ctxProgress =>
{

    var task = ctxProgress.AddTask("[cyan]Generating Audio[/]", maxValue: lines.Length);

    for (int i = 0; i < lines.Length; i++)
    {
        var line = lines[i].Trim();
        if (string.IsNullOrWhiteSpace(line))
        {
            task.Increment(1);
            continue;
        }

        var settings = (i % 2 == 0) ? hostSettings : guestSettings;

        AnsiConsole.MarkupLine($"[cyan]Processing line {i + 1}: " +
                $"{line.Substring(0, Math.Min(line.Length, 30))}[/]");

        var audioResult = await textToAudioService.GetAudioContentAsync(line, settings);

        if (!audioResult.Data.Value.IsEmpty)
        {
            finalAudioBytes.AddRange(audioResult.Data.Value.ToArray());
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]Failed to generate audio for line {i + 1}[/]");
        }
        task.Increment(1);
    }

});

var outputPath = "D:\\tests";
if (!Directory.Exists(outputPath))
{
    Directory.CreateDirectory(outputPath);
}

var finalAudioPath = Path.Combine(outputPath, "final_audio.mp3");
await File.WriteAllBytesAsync(finalAudioPath, finalAudioBytes.ToArray());

var audioPanel = new Panel(new Markup($"[bold green]Audio file generated at:[/]{finalAudioPath}"))
    .Header("[bold cyan]Audio Generated[/]")
    .BorderColor(Color.Cyan1)
    .RoundedBorder();

AnsiConsole.Write(audioPanel);

AnsiConsole.Write(new Rule("[bold yellow]Podcast Generation Completed![/]")
    .RuleStyle("bold yellow"));