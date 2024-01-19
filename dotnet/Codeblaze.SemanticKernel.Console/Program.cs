using System.Text.Json;
using Codeblaze.SemanticKernel.Console.Services;
using Codeblaze.SemanticKernel.Plugins.Neo4j;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.Memory;
using Spectre.Console;
using Spectre.Console.Json;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();
    
AnsiConsole.Write(new FigletText($"{config["Name"]!}").Color(Color.Green));
AnsiConsole.WriteLine("");

const string prompt = "1.\tPrompt kernel";
const string memory = "2.\tMemory search";
const string exit = "2.\tExit";

Run();

return;

void Run()
{
    while (true)
    {
        var option = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select an option")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
                .AddChoices(prompt, memory, exit)
        );

        switch (option)
        {
            case prompt:
                Prompt().GetAwaiter().GetResult();
                break;
            case memory:
                Memory().GetAwaiter().GetResult();
                break;
            case exit:
                return;
        }
    }
}

async Task Prompt()
{
    KernelService kernel = null;

    AnsiConsole.Status().Start("Initializing...", ctx =>
    {
        ctx.Spinner(Spinner.Known.Star);
        ctx.SpinnerStyle(Style.Parse("green"));

        kernel = new KernelService(config);

        ctx.Status("Initialized");
    });
    
    var prompt = AnsiConsole.Prompt(new TextPrompt<string>("What are you looking to do today?\n").PromptStyle("teal"));

    string result = null;
    
    await AnsiConsole.Status().StartAsync("Processing...", async ctx =>
    {
        ctx.Spinner(Spinner.Known.Star);
        ctx.SpinnerStyle(Style.Parse("green"));

        ctx.Status($"Processing input to generate cypher");
        result = await kernel.BasicPrompt(prompt);
    });

    AnsiConsole.Write(new Rule("[cyan][/]") { Justification = Justify.Center });
    AnsiConsole.WriteLine($"Result: {result}");
    AnsiConsole.Write(new Rule("[cyan][/]") { Justification = Justify.Center });
}

#pragma warning disable SKEXP0003
async Task Memory()
{
    NeoMemoryService memory = null;
    
    AnsiConsole.Status().Start("Initializing...", ctx =>
    {
        ctx.Spinner(Spinner.Known.Star);
        ctx.SpinnerStyle(Style.Parse("green"));

        memory = new NeoMemoryService(config);

        ctx.Status("Initialized");
    });
    
    var prompt = AnsiConsole.Prompt(new TextPrompt<string>("What are you looking to do today?\n").PromptStyle("teal"));

    IAsyncEnumerable<MemoryQueryResult> result = null;
    
    await AnsiConsole.Status().StartAsync("Processing...", async ctx =>
    {
        ctx.Spinner(Spinner.Known.Star);
        ctx.SpinnerStyle(Style.Parse("green"));

        ctx.Status($"Processing input to generate cypher");
        result = memory.Run(prompt);
    });

    await foreach (var record in result)
    {
        AnsiConsole.Write(new Rule("[cyan][/]") { Justification = Justify.Center });
        AnsiConsole.WriteLine($"Relevance : {record.Relevance}");
        AnsiConsole.WriteLine($"Node ID: {record.Metadata.Id}");
        AnsiConsole.WriteLine($"Node text: {record.Metadata.Text}");
    }
    
    AnsiConsole.Write(new Rule("[cyan][/]") { Justification = Justify.Center });
}
#pragma warning enable SKEXP0003