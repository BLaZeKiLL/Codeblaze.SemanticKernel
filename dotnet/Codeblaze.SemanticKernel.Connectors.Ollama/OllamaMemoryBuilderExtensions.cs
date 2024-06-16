using Microsoft.SemanticKernel.Memory;

namespace Codeblaze.SemanticKernel.Connectors.Ollama;

#pragma warning disable SKEXP0001
public static class OllamaMemoryBuilderExtensions
{
    /// <summary>
    /// Adds Ollama as the text embedding generation backend for semantic memory
    /// </summary>
    /// <param name="builder">kernel builder</param>
    /// <param name="modelId">Ollama model ID to use</param>
    /// <param name="baseUrl">Ollama base url</param>
    /// <returns></returns>
    public static MemoryBuilder WithOllamaTextEmbeddingGeneration(
        this MemoryBuilder builder, 
        string modelId,
        string baseUrl
    )
    {
        builder.WithTextEmbeddingGeneration((logger, http) => new OllamaTextEmbeddingGeneration(
            modelId,
            baseUrl,
            http,
            logger
        ));

        return builder;
    }

    /// <summary>
    /// Adds Ollama as the text embedding generation backend for semantic memory
    /// </summary>
    /// <param name="builder">kernel builder</param>
    /// <param name="modelId">Ollama model ID to use</param>
    /// <param name="baseUrl">Ollama base url</param>
    /// <returns></returns>
    public static MemoryBuilder WithOllamaTextEmbeddingGeneration(
        this MemoryBuilder builder, 
        string modelId,
        Uri baseUrl
    )
    {
        builder.WithTextEmbeddingGeneration((logger, http) => new OllamaTextEmbeddingGeneration(
            modelId,
            baseUrl.AbsoluteUri,
            http,
            logger
        ));

        return builder;
    }
}
#pragma warning enable SKEXP0001