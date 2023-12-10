using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.AI.Embeddings;
using Microsoft.SemanticKernel.AI.TextGeneration;

namespace Codeblaze.SemanticKernel.Connectors.AI.Ollama;

public static class OllamaKernelBuilderExtensions
{
    /// <summary>
    /// Adds Ollama as the text generation llm backend for semantic kernel
    /// </summary>
    /// <param name="builder">kernel builder</param>
    /// <param name="modelId">Ollama model ID to use</param>
    /// <param name="baseUrl">Ollama base url</param>
    /// <param name="serviceId"></param>
    /// <returns></returns>
    public static IKernelBuilder AddOllamaTextGeneration(
        this IKernelBuilder builder,
        string modelId,
        string baseUrl,
        string? serviceId = null
    )
    {
        var factory = (IServiceProvider provider, object? _) => new OllamaTextGenerationService(
            modelId, 
            baseUrl, 
            provider.GetRequiredService<HttpClient>(), 
            provider.GetService<ILoggerFactory>()
        );

        builder.Services.AddKeyedSingleton<ITextGenerationService>(serviceId, factory);

        return builder;
    }

    /// <summary>
    /// Adds Ollama as the text generation llm backend for semantic kernel
    /// </summary>
    /// <param name="builder">kernel builder</param>
    /// <param name="modelId">Ollama model ID to use</param>
    /// <param name="baseUrl">Ollama base url</param>
    /// <param name="serviceId"></param>
    /// <returns></returns>
    public static IKernelBuilder AddOllamaTextGeneration(
        this IKernelBuilder builder, 
        string modelId, 
        Uri baseUrl, 
        string? serviceId = null
    )
    {
        var factory = (IServiceProvider provider, object? _) => new OllamaTextGenerationService(
            modelId, 
            baseUrl.AbsoluteUri, 
            provider.GetRequiredService<HttpClient>(), 
            provider.GetService<ILoggerFactory>()
        );

        builder.Services.AddKeyedSingleton<ITextGenerationService>(serviceId, factory);

        return builder;
    }
    
    /// <summary>
    /// Adds Ollama as the chat completion llm backend for semantic kernel
    /// </summary>
    /// <param name="builder">kernel builder</param>
    /// <param name="modelId">Ollama model ID to use</param>
    /// <param name="baseUrl">Ollama base url</param>
    /// <param name="serviceId"></param>
    /// <returns></returns>
    public static IKernelBuilder AddOllamaChatCompletion(
        this IKernelBuilder builder,
        string modelId,
        string baseUrl,
        string? serviceId = null
    )
    {
        var factory = (IServiceProvider provider, object? _) => new OllamaChatCompletionService(
            modelId, 
            baseUrl, 
            provider.GetRequiredService<HttpClient>(), 
            provider.GetService<ILoggerFactory>()
        );

        builder.Services.AddKeyedSingleton<IChatCompletionService>(serviceId, factory);

        return builder;
    }
    
    /// <summary>
    /// Adds Ollama as the chat completion llm backend for semantic kernel
    /// </summary>
    /// <param name="builder">kernel builder</param>
    /// <param name="modelId">Ollama model ID to use</param>
    /// <param name="baseUrl">Ollama base url</param>
    /// <param name="serviceId"></param>
    /// <returns></returns>
    public static IKernelBuilder AddOllamaChatCompletion(
        this IKernelBuilder builder,
        string modelId,
        Uri baseUrl,
        string? serviceId = null
    )
    {
        var factory = (IServiceProvider provider, object? _) => new OllamaChatCompletionService(
            modelId, 
            baseUrl.AbsoluteUri, 
            provider.GetRequiredService<HttpClient>(), 
            provider.GetService<ILoggerFactory>()
        );

        builder.Services.AddKeyedSingleton<IChatCompletionService>(serviceId, factory);

        return builder;
    }
    
    /// <summary>
    /// Adds Ollama as the text embedding generation backend for semantic kernel
    /// </summary>
    /// <param name="builder">kernel builder</param>
    /// <param name="modelId">Ollama model ID to use</param>
    /// <param name="baseUrl">Ollama base url</param>
    /// <param name="serviceId"></param>
    /// <returns></returns>
    public static IKernelBuilder AddOllamaTextEmbeddingGeneration(
        this IKernelBuilder builder,
        string modelId,
        string baseUrl,
        string? serviceId = null
    )
    {
        var factory = (IServiceProvider provider, object? _) => new OllamaTextEmbeddingGeneration(
            modelId,
            baseUrl,
            provider.GetRequiredService<HttpClient>(),
            provider.GetService<ILoggerFactory>()
        );

#pragma warning disable SKEXP0001
        builder.Services.AddKeyedSingleton<ITextEmbeddingGeneration>(serviceId, factory);
#pragma warning restore SKEXP0001
        return builder;
    }
    
    /// <summary>
    /// Adds Ollama as the text embedding generation backend for semantic kernel
    /// </summary>
    /// <param name="builder">kernel builder</param>
    /// <param name="modelId">Ollama model ID to use</param>
    /// <param name="baseUrl">Ollama base url</param>
    /// <param name="serviceId"></param>
    /// <returns></returns>
    public static IKernelBuilder AddOllamaTextEmbeddingGeneration(
        this IKernelBuilder builder,
        string modelId,
        Uri baseUrl,
        string? serviceId = null
    )
    {
        var factory = (IServiceProvider provider, object? _) => new OllamaTextEmbeddingGeneration(
            modelId,
            baseUrl.AbsoluteUri,
            provider.GetRequiredService<HttpClient>(),
            provider.GetService<ILoggerFactory>()
        );

#pragma warning disable SKEXP0001
        builder.Services.AddKeyedSingleton<ITextEmbeddingGeneration>(serviceId, factory);
#pragma warning restore SKEXP0001
        return builder;
    }
}