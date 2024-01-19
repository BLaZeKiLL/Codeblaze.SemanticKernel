using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Codeblaze.SemanticKernel.Connectors.Ollama;

public class OllamaChatCompletionService(
    string modelId,
    string baseUrl,
    HttpClient http,
    ILoggerFactory? loggerFactory)
    : OllamaBase<OllamaChatCompletionService>(modelId, baseUrl, http, loggerFactory), IChatCompletionService
{
    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null, CancellationToken cancellationToken = new())
    {
        var chat = string.Join("\n", chatHistory.Select(x => x.Content));

        var data = new
        {
            model = Attributes["model_id"] as string,
            prompt = chat,
            stream = false,
            options = executionSettings?.ExtensionData,
        };

        var response = await Http.PostAsJsonAsync($"{Attributes["base_url"]}/api/generate", data, cancellationToken).ConfigureAwait(false);

        ValidateOllamaResponse(response);

        var json = JsonSerializer.Deserialize<JsonNode>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));

        return new List<ChatMessageContent> { new(AuthorRole.Assistant, json!["response"]!.GetValue<string>(), modelId: Attributes["model_id"] as string) };
    }

    public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null, Kernel? kernel = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = new())
    {
        var chat = string.Join("\n", chatHistory.Select(x => x.Content));

        var data = new
        {
            model = Attributes["model_id"] as string,
            prompt = chat,
            stream = true,
            options = executionSettings?.ExtensionData,
        };

        var response = await Http.PostAsJsonAsync($"{Attributes["base_url"]}/api/generate", data, cancellationToken).ConfigureAwait(false);

        ValidateOllamaResponse(response);

        using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

        using var reader = new StreamReader(stream);

        var done = false;

        while (!done)
        {
            var json = JsonSerializer.Deserialize<JsonNode>(
                await response.Content.ReadAsStringAsync().ConfigureAwait(false)
            );

            done = json!["done"]!.GetValue<bool>();

            yield return new StreamingChatMessageContent(AuthorRole.Assistant, json["response"]!.GetValue<string>(), modelId: Attributes["model_id"] as string);
        }
    }
}