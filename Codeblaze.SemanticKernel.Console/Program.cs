using Codeblaze.SemanticKernel.Console.Services;
using Microsoft.Extensions.Configuration;
using Spectre.Console;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();
    
AnsiConsole.Write(new FigletText($"{config["Name"]!}").Color(Color.Green));
AnsiConsole.WriteLine("");

KernelService kernel = null;

AnsiConsole.Status().Start("Initializing...", ctx =>
{
    ctx.Spinner(Spinner.Known.Star);
    ctx.SpinnerStyle(Style.Parse("green"));

    kernel = new KernelService(config);

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

    var result = string.Empty;
    
    await AnsiConsole.Status().StartAsync("Processing...", async ctx =>
    {
        ctx.Spinner(Spinner.Known.Star);
        ctx.SpinnerStyle(Style.Parse("green"));

        ctx.Status($"Processing input to generate Chat Response");
        result = await kernel.BasicPrompt(prompt);
    });
    
    AnsiConsole.WriteLine("");
    AnsiConsole.Write(new Rule($"[silver]AI Assistant Response[/]") { Justification = Justify.Center });
    AnsiConsole.WriteLine(result);
    AnsiConsole.Write(new Rule($"[yellow]****[/]") { Justification = Justify.Center });
    AnsiConsole.WriteLine("");
}