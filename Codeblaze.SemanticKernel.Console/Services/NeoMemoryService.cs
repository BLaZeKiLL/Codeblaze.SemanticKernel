using Codeblaze.SemanticKernel.Connectors.Memory.Neo4j;
using Codeblaze.SemanticKernel.Connectors.Ollama;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;

namespace Codeblaze.SemanticKernel.Console.Services;

#pragma warning disable SKEXP0003
public class NeoMemoryService
{
    private readonly ISemanticTextMemory _memory;

    public NeoMemoryService(IConfiguration config)
    {
        var builder = new MemoryBuilder();

        builder.WithHttpClient(new HttpClient());
        builder.WithOllamaTextEmbeddingGeneration(config["Ollama:Model"], config["Ollama:BaseUrlEmbeddings"]);
        builder.WithNeo4jMemoryStore(config["Neo4j:Url"], config["Neo4j:Username"], config["Neo4j:Password"],
            new Neo4jVectorIndexQueryFactory(
                "top_questions",
                "Question",
                "embedding",
                384
            ));

        _memory = builder.Build();
    }

    public IAsyncEnumerable<MemoryQueryResult> Run(string prompt)
    {
        return _memory.SearchAsync("top_questions", prompt, limit: 3);
    }
}
#pragma warning enable SKEXP0003