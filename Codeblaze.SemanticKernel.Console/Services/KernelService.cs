using Codeblaze.SemanticKernel.Connectors.AI.Ollama;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Codeblaze.SemanticKernel.Console.Services;

public class KernelService
{
    private readonly Kernel _Kernel;

    public KernelService(IConfiguration config)
    {
        var builder = Kernel.CreateBuilder();

        builder.Services.AddTransient<HttpClient>();

        builder
            .AddOllamaTextGeneration(config["Ollama:Model"], config["Ollama:BaseUrlGeneration"]);

        _Kernel = builder.Build();
    }
    
    public async Task<string> BasicPrompt(string input)
    {
        const string prompt = """
        Bot: How can I help you?
        User: {{$input}}
                            
        ---------------------------------------------
                            
        The intent of the user in 5 words or less:
        """;
        
        var result = await _Kernel.InvokePromptAsync(prompt, new KernelArguments
        {
            {"input", input}
        });

        return result.GetValue<string>();
    }

    public async Task<string> BasicChat(string input)
    {
        var chat = _Kernel.GetRequiredService<IChatCompletionService>();

        var history = new ChatHistory();
        
        history.AddSystemMessage("...");
        history.AddAssistantMessage("...");
        history.AddUserMessage(input);

        var result = await chat.GetChatMessageContentsAsync(history);

        return result[^1].Content;
    }
}