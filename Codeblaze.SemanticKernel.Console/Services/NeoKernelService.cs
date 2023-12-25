using Codeblaze.SemanticKernel.Connectors.Ollama;
using Codeblaze.SemanticKernel.Plugins.Neo4j;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Codeblaze.SemanticKernel.Console.Services;

public class NeoKernelService
{
    private readonly Kernel _Kernel;

    public NeoKernelService(IConfiguration config)
    {
        var builder = Kernel.CreateBuilder();

        // builder.Services.AddTransient<HttpClient>();
        
        builder.AddOpenAIChatCompletion(config["OpenAI:Model"], config["OpenAI:Key"]);
        // builder.AddOllamaChatCompletion(config["Ollama:Model"], config["Ollama:BaseUrlGeneration"]);
        
        _Kernel = builder.Build();
        
        _Kernel.AddNeo4jCypherGenPlugin(config["Neo4j:Url"], config["Neo4j:Username"], config["Neo4j:Password"]);
    }
    
    public Task<Neo4jResult?> Run(string prompt)
    {
        return _Kernel.InvokeAsync<Neo4jResult>(
            nameof(Neo4jCypherGenPlugin), "Query", 
            new KernelArguments
            {
                { "prompt", prompt }
            }
        );
    }
}