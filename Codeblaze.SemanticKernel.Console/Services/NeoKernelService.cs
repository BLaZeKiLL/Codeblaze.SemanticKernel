using Codeblaze.SemanticKernel.Plugins.Neo4j;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;

namespace Codeblaze.SemanticKernel.Console.Services;

public class NeoKernelService
{
    private readonly Kernel _Kernel;
    private readonly Neo4jPlugin _plugin;

    public NeoKernelService(IConfiguration config)
    {
        var builder = Kernel.CreateBuilder();

        builder.AddOpenAIChatCompletion(config["OpenAI:Model"], config["OpenAI:Key"]);

        _Kernel = builder.Build();

        _plugin = new Neo4jPlugin(_Kernel, config["Neo4j:Url"], config["Neo4j:Username"], config["Neo4j:Password"]);
    }

    public string GetSchema()
    {
        return _plugin.Schema;
    }

    public Task<string> GenerateCypher(string prompt)
    {
        return _plugin.GenerateCypher(prompt);
    }
    
    public Task<NeoResult> Run(string prompt)
    {
        return _plugin.Run(prompt);
    }
}