using System.Text.Json.Serialization;

namespace Codeblaze.SemanticKernel.Connectors.Ollama
{
    internal class OllamaResponseMessage
    {
        /// <summary>
        /// The model used to generate the response.
        /// </summary>
        [JsonPropertyName("model")]
        public string Model { get; set; } = null!;

        /// <summary>
        /// The message created date.
        /// </summary>
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Value indicating whether the message is done.
        /// </summary>
        [JsonPropertyName("done")]
        public bool Done { get; set; } = false!;

        /// <summary>
        /// The time spent generating the response.
        /// </summary>
        [JsonPropertyName("total_duration")]
        public UInt64 TotalDuration { get; set; } = 0;

        /// <summary>
        /// The time spent in nanoseconds loading the model.
        /// </summary>
        [JsonPropertyName("load_duration")]
        public UInt64 LoadDuration { get; set; } = 0;

        /// <summary>
        /// The number of tokens in the prompt.
        /// </summary>
        [JsonPropertyName("generate_duration")]
        public int PromptEvalCount { get; set; } = 0;

        /// <summary>
        /// The time spent in nanoseconds evaluating the prompt.
        /// </summary>
        [JsonPropertyName("prompt_eval_count")]
        public UInt64 PromptEvalDuration { get; set; } = 0;

        /// <summary>
        /// The number of tokens the response.
        /// </summary>
        [JsonPropertyName("eval_count")]
        public int EvalCount { get; set; } = 0;

        /// <summary>
        /// The time in nanoseconds spent generating the response.
        /// </summary>
        [JsonPropertyName("eval_duration")]
        public UInt64 EvalDuration { get; set; }
    }
}
