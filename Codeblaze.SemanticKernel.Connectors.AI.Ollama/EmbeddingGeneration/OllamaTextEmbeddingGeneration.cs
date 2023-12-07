using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.Embeddings;

namespace Codeblaze.SemanticKernel.Connectors.AI.Ollama;

#pragma warning disable SKEXP0001
public class OllamaTextEmbeddingGeneration(string modelId, string baseUrl, HttpClient http, ILoggerFactory? loggerFactory)
    : OllamaBase<OllamaTextEmbeddingGeneration>(modelId, baseUrl, http, loggerFactory),
        ITextEmbeddingGeneration
#pragma warning restore SKEXP0001
{
    public async Task<IList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(IList<string> data, Kernel? kernel = null,
        CancellationToken cancellationToken = new())
    {
        var result = new List<ReadOnlyMemory<float>>(data.Count);
        
        foreach (var text in data)
        {
            var request = new
            {
                model = Attributes["model_id"],
                prompt = text
            };

            var response = await Http.PostAsJsonAsync($"{Attributes["base_url"]}/api/embeddings", request, cancellationToken).ConfigureAwait(false);

            ValidateOllamaResponse(response);

            var json = JsonSerializer.Deserialize<JsonNode>(await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));

            var embedding = new ReadOnlyMemory<float>(json!["embedding"]?.AsArray().GetValues<float>().ToArray());
            
            result.Add(embedding);
        }

        return result;
    }
}