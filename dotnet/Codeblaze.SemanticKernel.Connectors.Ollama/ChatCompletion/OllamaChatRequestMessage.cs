using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json.Serialization;

namespace Codeblaze.SemanticKernel.Connectors.Ollama.ChatCompletion
{
    internal abstract class OllamaChatRequestMessage
    {
        /// <summary>
        /// The role of the author of the message either "system", "user", or "assistant".
        /// </summary>
        [JsonPropertyName("role")]
        public string Role { get; protected set; } = null!;

        /// <summary>
        ///The content of the message
        /// </summary>
        [JsonPropertyName("content")]
        public string Content { get; set; } = null!;

        internal ChatMessageContent ToChatMessageContent(string modelId = null!)
        {
            return new ChatMessageContent(new AuthorRole(Role), Content, modelId: modelId);
        }
    }

    internal sealed class OllamaChatRequestSystemMessage : OllamaChatRequestMessage
    {
        public OllamaChatRequestSystemMessage(string content)
        {
            Role = "system";
            Content = content;
        }
    }

    internal sealed class OllamaChatRequestUserMessage : OllamaChatRequestMessage
    {
        public OllamaChatRequestUserMessage(string content)
        {
            Role = "user";
            Content = content;
        }
    }

    internal sealed class OllamaChatRequestAssistantMessage : OllamaChatRequestMessage
    {
        public OllamaChatRequestAssistantMessage(string content)
        {
            Role = "assistant";
            Content = content;
        }
    }
}
