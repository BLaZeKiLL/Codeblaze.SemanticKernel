using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Codeblaze.SemanticKernel.Connectors.AI.Ollama;

public interface IOllamaBase
{
    Task PingOllamaAsync(CancellationToken cancellationToken = new());
}

public abstract class OllamaBase<T> : IOllamaBase where T : OllamaBase<T>
{
    public IReadOnlyDictionary<string, object?> Attributes => _attributes;
    
    private readonly Dictionary<string, object?> _attributes = new();
    
    protected readonly HttpClient Http;
    protected readonly ILogger<T> Logger;

    protected OllamaBase(string modelId, string baseUrl, HttpClient http, ILoggerFactory? loggerFactory)
    {
        _attributes.Add("model_id", modelId);
        _attributes.Add("base_url", baseUrl);

        Http = http;
        Logger = loggerFactory is not null ? loggerFactory.CreateLogger<T>() : NullLogger<T>.Instance;
    }

    /// <summary>
    /// Ping Ollama instance to check if the required llm model is available at the instance
    /// </summary>
    /// <param name="cancellationToken"></param>
    public async Task PingOllamaAsync(CancellationToken cancellationToken = new())
    {
        var data = new
        {
            name = Attributes["model_id"]
        };
        
        var response = await Http.PostAsJsonAsync($"{Attributes["base_url"]}/api/show", data, cancellationToken).ConfigureAwait(false);

        ValidateOllamaResponse(response);
        
        Logger.LogInformation("Connected to Ollama at {url} with model {model}", Attributes["base_url"], Attributes["model_id"]);
    }

    protected void ValidateOllamaResponse(HttpResponseMessage? response)
    {
        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException)
        {
            Logger.LogError("Unable to connect to ollama at {url} with model {model}", Attributes["base_url"], Attributes["model_id"]);
        }
    }
}