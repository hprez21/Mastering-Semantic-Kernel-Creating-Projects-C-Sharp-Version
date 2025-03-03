#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0050

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using SKVectorStoreTextSearchDemo;
using SKVectorStoreTextSearchDemo.Models;

var openAIKey = Environment.GetEnvironmentVariable("SKCourseOpenAIKey");
var azureRegion = Environment.GetEnvironmentVariable("SKCourseAzureRegion");
var azureKey = Environment.GetEnvironmentVariable("SKCourseAzureKey");
 
ITextEmbeddingGenerationService openAIEmbeddings =
    new OpenAITextEmbeddingGenerationService("text-embedding-ada-002", openAIKey);

ITextEmbeddingGenerationService azureOpenAIEmbeddings = new AzureOpenAITextEmbeddingGenerationService(
    "text-embedding-ada-002",
    $"{azureRegion}",
    $"{azureKey}");

#region Generating embeddings and saving in Vector Stores

var vectorStore =
    new InMemoryVectorStore();

var collection = vectorStore.GetCollection<string, Glossary>("skglossary");

await collection.CreateCollectionIfNotExistsAsync();


var glossaryEntries = CreateGlossaryEntries().ToList();

var tasks = glossaryEntries.Select(entry => Task.Run(async () =>
{
        entry.DefinitionEmbedding = await azureOpenAIEmbeddings.GenerateEmbeddingAsync(entry.Definition);
}));

await Task.WhenAll(tasks);

var upsertedKeysTasks = glossaryEntries.Select(x => collection.UpsertAsync(x));
await Task.WhenAll(upsertedKeysTasks);

//var record = await collection.GetAsync("100");
//Console.WriteLine(record!.Definition);

#endregion

#region Performing Vector Search

//string searchString = "What is an API?";

//var searchVector = await azureOpenAIEmbeddings.GenerateEmbeddingAsync(searchString);

//var filter = new VectorSearchFilter()
//    .EqualTo(nameof(Glossary.Category), "AI");

//var searchResult =
//    await collection.VectorizedSearchAsync(
//            searchVector,
//            new()
//            {
//                Filter = filter,
//            }
//        );

//var searchResultItems = await searchResult.Results.ToListAsync();

//var searchResultItem = searchResultItems.FirstOrDefault();
//Console.WriteLine(searchResultItem.Record.Definition);
//Console.WriteLine(searchResultItem.Score);

#endregion


var openAiKernelBuilder = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion("gpt-4o-mini-2024-07-18", $"{openAIKey}");

openAiKernelBuilder.Services.AddSingleton<IFunctionInvocationFilter, FunctionInvocationFilter>();

var openAIKernel = openAiKernelBuilder.Build();


Kernel azureKernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion("gpt-4o-mini",
        $"{azureRegion}",
        $"{azureKey}")
    .Build();

var bingKey = Environment.GetEnvironmentVariable("BingKey");

 
//var textSearch = new BingTextSearch(bingKey);

#region Text Search

//KernelSearchResults<string> searchResults = await textSearch.SearchAsync("What is an API?",
//    new() { Top = 4});

//await foreach(string result in searchResults.Results)
//{
//    Console.WriteLine(result);
//}


//KernelSearchResults<object> searchResults = await textSearch.GetSearchResultsAsync("What is an API?",
//new() { Top = 4 });

//await foreach (BingWebPage webPage in searchResults.Results)
//{
//    Console.WriteLine($"Name:            {webPage.Name}");
//    Console.WriteLine($"Snippet:         {webPage.Snippet}");
//    Console.WriteLine($"Url:             {webPage.Url}");
//    Console.WriteLine($"DisplayUrl:      {webPage.DisplayUrl}");
//    Console.WriteLine($"DateLastCrawled: {webPage.DateLastCrawled}");
//}

//KernelSearchResults<TextSearchResult> textResults = 
//        await textSearch.GetTextSearchResultsAsync("What is an API?", new() { Top = 4 });

//await foreach (TextSearchResult result in textResults.Results)
//{
//    Console.WriteLine($"Name:  {result.Name}");
//    Console.WriteLine($"Value: {result.Value}");
//    Console.WriteLine($"Link:  {result.Link}");
//}

#endregion

#region Text Search Plugins

//var searchPlugin =
//    textSearch.CreateWithSearch("SearchPlugin");

//openAIKernel.Plugins.Add(searchPlugin);

//var query = "What is the Semantic Kernel?";

//var prompt = "{{SearchPlugin.Search $query}}. {{$query}}";

//KernelArguments arguments = new()
//{
//    { "query", query }
//};

//Console.WriteLine(await openAIKernel.InvokePromptAsync(prompt, arguments));

//Example 2

//var searchPlugin = textSearch.CreateWithGetTextSearchResults("SearchPlugin");

//openAIKernel.Plugins.Add(searchPlugin);

//var query = "What is the Semantic Kernel?";

//string promptTemplate = """
//{{#with (SearchPlugin-GetTextSearchResults query)}}  
//    {{#each this}}  
//    Name: {{Name}}
//    Value: {{Value}}
//    Link: {{Link}}
//    -----------------
//    {{/each}}  
//{{/with}}  

//{{query}}

//Include citations to the relevant information where it is referenced in the response.
//""";


//KernelArguments arguments = new()
//{
//    { "query", query }
//};


//var promptTemplateFactory = new HandlebarsPromptTemplateFactory();

//Console.WriteLine(await openAIKernel.InvokePromptAsync(promptTemplate, arguments, 
//    templateFormat: HandlebarsPromptTemplateFactory.HandlebarsTemplateFormat,
//    promptTemplateFactory: promptTemplateFactory));

#endregion

#region Text Search Plugins - Function Calling

//// Create a kernel with OpenAI chat completion
//var searchPlugin = textSearch.CreateWithSearch("SearchPlugin");
//openAIKernel.Plugins.Add(searchPlugin);


//OpenAIPromptExecutionSettings settings =
//    new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };

//// Invoke prompt and use text search plugin to provide grounding information
//var query = "What is the Semantic Kernel?";

////var prompt = "{{SearchPlugin.Search $query}}. {{$query}}";
//KernelArguments arguments = new(settings);
//Console.WriteLine(await openAIKernel.InvokePromptAsync(query, arguments));

//Example 2

//// Create a kernel with OpenAI chat completion
//var searchPlugin = textSearch.CreateWithGetTextSearchResults("SearchPlugin");
//openAIKernel.Plugins.Add(searchPlugin);


//OpenAIPromptExecutionSettings settings =
//    new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };

//// Invoke prompt and use text search plugin to provide grounding information
//var query = "What is the Semantic Kernel? Include citations to the relevant information where it is referenced in the response.";

////var prompt = "{{SearchPlugin.Search $query}}. {{$query}}";
//KernelArguments arguments = new(settings);
//Console.WriteLine(await openAIKernel.InvokePromptAsync(query, arguments));

#endregion



var textSearch = new VectorStoreTextSearch<Glossary>(collection, azureOpenAIEmbeddings);

var query = "What is RAG?";

KernelSearchResults<TextSearchResult> textResults =
    await textSearch.GetTextSearchResultsAsync(query, new() { Top = 2, Skip = 0});


Console.WriteLine("\n--- Text Search Results ---\n");
await foreach (TextSearchResult result in textResults.Results)
{
    Console.WriteLine($"Name:  {result.Name}");
    Console.WriteLine($"Value: {result.Value}");
    Console.WriteLine($"Link:  {result.Link}");
}


IEnumerable<Glossary> CreateGlossaryEntries()
{
    yield return new Glossary
    {
        Key = "1",
        Category = "Software",
        Term = "API",
        Definition = "Application Programming Interface. A set of rules and specifications that allow software components to communicate and exchange data."
    };

    yield return new Glossary
    {
        Key = "2",
        Category = "Software",
        Term = "SDK",
        Definition = "Software development kit. A set of libraries and tools that allow software developers to build software more easily."
    };

    yield return new Glossary
    {
        Key = "3",
        Category = "SK",
        Term = "Connectors",
        Definition = "Semantic Kernel Connectors allow software developers to integrate with various services providing AI capabilities, including LLM, AudioToText, TextToAudio, Embedding generation, etc."
    };

    yield return new Glossary
    {
        Key = "4",
        Category = "SK",
        Term = "Semantic Kernel",
        Definition = "Semantic Kernel is a set of libraries that allow software developers to more easily develop applications that make use of AI experiences."
    };

    yield return new Glossary
    {
        Key = "5",
        Category = "AI",
        Term = "RAG",
        Definition = "Retrieval Augmented Generation - a term that refers to the process of retrieving additional data to provide as context to an LLM to use when generating a response (completion) to a user’s question (prompt)."
    };

    yield return new Glossary
    {
        Key = "6",
        Category = "AI",
        Term = "LLM",
        Definition = "Large language model. A type of artificial ingelligence algorithm that is designed to understand and generate human language."
    };
}
