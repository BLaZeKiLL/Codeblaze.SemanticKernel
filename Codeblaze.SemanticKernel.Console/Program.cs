using System.Text.Json;
using Codeblaze.SemanticKernel.Console.Services;
using Codeblaze.SemanticKernel.Plugins.Neo4j;
using Microsoft.Extensions.Configuration;
using Spectre.Console;
using Spectre.Console.Json;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();
    
AnsiConsole.Write(new FigletText($"{config["Name"]!}").Color(Color.Green));
AnsiConsole.WriteLine("");

NeoKernelService kernel = null;

AnsiConsole.Status().Start("Initializing...", ctx =>
{
    ctx.Spinner(Spinner.Known.Star);
    ctx.SpinnerStyle(Style.Parse("green"));

    kernel = new NeoKernelService(config);

    ctx.Status("Initialized");
});

const string prompt = "1.\tPrompt kernel";
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
                .AddChoices(prompt, exit)
        );

        switch (option)
        {
            case prompt:
                Prompt().GetAwaiter().GetResult();
                break;
            case exit:
                return;
        }
    }
}

async Task Prompt()
{
    var prompt = AnsiConsole.Prompt(new TextPrompt<string>("What are you looking to do today ?").PromptStyle("teal"));

    NeoResult result = null;
    
    await AnsiConsole.Status().StartAsync("Processing...", async ctx =>
    {
        ctx.Spinner(Spinner.Known.Star);
        ctx.SpinnerStyle(Style.Parse("green"));

        ctx.Status($"Processing input to generate cypher");
        result = await kernel.Run(prompt);
    });

    if (result.Success)
    {
        AnsiConsole.WriteLine("");
        AnsiConsole.Write(new Rule("[cyan]Cypher[/]") { Justification = Justify.Center });
        AnsiConsole.WriteLine(result.Cypher);
        AnsiConsole.Write(new Panel(new JsonText(JsonSerializer.Serialize(result.Result)))
            .Header("Cypher Result")
            .Expand()
            .RoundedBorder()
            .BorderColor(Color.Green));
        AnsiConsole.WriteLine("");
    }
    else
    {
        AnsiConsole.WriteLine("");
        AnsiConsole.Write(new Rule("[red]Cypher[/]") { Justification = Justify.Center });
        AnsiConsole.WriteLine(result.Cypher);
        AnsiConsole.Write(new Rule("[red]Cypher Execution Error[/]") { Justification = Justify.Center });
        AnsiConsole.WriteLine("");
    }
    

}