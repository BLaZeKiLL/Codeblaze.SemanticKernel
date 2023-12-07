using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.AI.TextGeneration;

namespace Codeblaze.SemanticKernel.Connectors.AI.Ollama;

public class OllamaTextGenerationService(string modelId, string baseUrl, HttpClient http, ILoggerFactory? loggerFactory)
    : OllamaBase<OllamaTextGenerationService>(modelId, baseUrl, http, loggerFactory), ITextGenerationService
{
    public async Task<IReadOnlyList<TextContent>> GetTextContentsAsync(string prompt,
        PromptExecutionSettings? executionSettings = null, Kernel? kernel = null,
        CancellationToken cancellationToken = new())
    {
        var data = new
        {
            model = Attributes["model_id"],
            prompt,
            stream = false,
            options = executionSettings?.ExtensionData,
        };

        var response = await Http.PostAsJsonAsync($"{Attributes["base_url"]}/api/generate", data, cancellationToken).ConfigureAwait(false);

        ValidateOllamaResponse(response);

        var json = JsonSerializer.Deserialize<JsonNode>(await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));

        return new List<TextContent> { new(json!["response"]!.GetValue<string>()) };
    }

    public async IAsyncEnumerable<StreamingTextContent> GetStreamingTextContentsAsync(string prompt,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null, [EnumeratorCancellation] CancellationToken cancellationToken = new())
    {
        var data = new
        {
            model = Attributes["model_id"],
            prompt,
            stream = true,
            options = executionSettings?.ExtensionData,
        };

        var response = await Http.PostAsJsonAsync($"{Attributes["base_url"]}/api/generate", data, cancellationToken).ConfigureAwait(false);

        ValidateOllamaResponse(response);
        
        var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        await using (stream)
        {
            using var reader = new StreamReader(stream);

            var done = false;

            while (!done)
            {
                var json = JsonSerializer.Deserialize<JsonNode>(
                    await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false)
                );

                done = json!["done"]!.GetValue<bool>();

                yield return new StreamingTextContent(json["response"]!.GetValue<string>());
            }
        }
    }
}