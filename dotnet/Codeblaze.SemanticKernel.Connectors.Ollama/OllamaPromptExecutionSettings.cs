using Microsoft.SemanticKernel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Codeblaze.SemanticKernel.Connectors.Ollama
{
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public sealed class OllamaPromptExecutionSettings
    {
        /// <summary>
        /// Temperature controls the randomness of the completion.
        /// The higher the temperature, the more random the completion.
        /// Default is 1.0.
        /// </summary>
        [JsonPropertyName("temperature")]
        public double Temperature { get; set; } = .8;

        /// <summary>
        /// Works together with top-k. A higher value (e.g., 0.95) will lead to more diverse text, while a lower value (e.g., 0.5) will generate more focused and conservative text. 
        /// Default: 0.9
        /// </summary>
        [JsonPropertyName("top_p")]
        public double TopP { get; set; } = .9;

        /// <summary>
        /// Reduces the probability of generating nonsense. A higher value (e.g. 100) will give more diverse answers, while a lower value (e.g. 10) will be more conservative. 
        /// Default: 40
        /// </summary>
        [JsonPropertyName("top_k")]
        public int TopK { get; set; } = 40;

        /// <summary>
        /// Maximum number of tokens to predict when generating text. (Default: 128, -1 = infinite generation, -2 = fill context)
        /// </summary>
        [JsonPropertyName("num_predict")]
        public int? MaxTokens { get; set; }


        internal static OllamaPromptExecutionSettings FromExecutionSettings(PromptExecutionSettings? executionSettings, int? defaultMaxTokens = null)
        {
            if (executionSettings is null)
            {
                return new OllamaPromptExecutionSettings()
                {
                    MaxTokens = defaultMaxTokens
                };
            }

            var json = JsonSerializer.Serialize(executionSettings);

            var ollamaExecutionSettings = JsonSerializer.Deserialize<OllamaPromptExecutionSettings>(json, new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            });

            if (ollamaExecutionSettings is not null)
            {
                return ollamaExecutionSettings;
            }

            throw new ArgumentException($"Invalid execution settings, cannot convert to {nameof(OllamaPromptExecutionSettings)}", nameof(executionSettings));
        }
    }
}
