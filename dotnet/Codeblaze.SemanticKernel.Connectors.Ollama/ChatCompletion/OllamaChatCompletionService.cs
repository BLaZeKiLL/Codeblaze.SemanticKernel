using Codeblaze.SemanticKernel.Connectors.Ollama.ChatCompletion;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;

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
        var chatExecutionSettings = OllamaPromptExecutionSettings.FromExecutionSettings(executionSettings);

        var data = new
        {
            model = Attributes["model_id"] as string,
            messages = GetChatMessages(chatHistory),
            stream = false,
            options = chatExecutionSettings
        };

        var response = await Http.PostAsJsonAsync($"{Attributes["base_url"]}/api/chat", data, cancellationToken).ConfigureAwait(false);

        ValidateOllamaResponse(response);
        string jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        var chatResponseMessage = JsonSerializer.Deserialize<OllamaChatResponseMessage>(jsonResponse);

        return new List<ChatMessageContent> { chatResponseMessage!.ToChatMessageContent() };
    }

    public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null, Kernel? kernel = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = new())
    {
        var chatExecutionSettings = OllamaPromptExecutionSettings.FromExecutionSettings(executionSettings);

        var data = new
        {
            model = Attributes["model_id"] as string,
            messages = GetChatMessages(chatHistory),
            stream = true,
            options = chatExecutionSettings
        };

        var request = new HttpRequestMessage(HttpMethod.Post, $"{Attributes["base_url"]}/api/chat");
        request.Content = JsonContent.Create(data);
        using var response = await Http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

        ValidateOllamaResponse(response);

        using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

        using var reader = new StreamReader(stream);

        var done = false;

        while (!done)
        {
            string jsonResponse = await reader.ReadLineAsync();

            var chatResponseMessage = JsonSerializer.Deserialize<OllamaChatResponseMessage>(jsonResponse);
            done = chatResponseMessage!.Done;

            yield return chatResponseMessage!.ToStreamingChatMessageContent();
        }
    }

    private IEnumerable<OllamaChatRequestMessage> GetChatMessages(ChatHistory chat)
    {
        foreach (var item in chat)
        {
            if (item.Role == AuthorRole.System)
                yield return new OllamaChatRequestSystemMessage(item.Content!);
            else if (item.Role == AuthorRole.User)
                yield return new OllamaChatRequestUserMessage(item.Content!);
            else if (item.Role == AuthorRole.Assistant)
                yield return new OllamaChatRequestAssistantMessage(item.Content!);
        }
    }
}