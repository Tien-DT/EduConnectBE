using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EduConnect.Application.DTOs.Response
{
    internal sealed class GeminiResponse
    {
        [JsonProperty("candidates")]
        public Candidate[] Candidates { get; set; }

        [JsonProperty("promptFeedback")]
        public PromptFeedback PromptFeedback { get; set; }
    }

    internal sealed class Candidate
    {
        [JsonProperty("content")]
        public Content Content { get; set; }

        [JsonProperty("finishReason")]
        public string FinishReason { get; set; }

        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("safetyRatings")]
        public SafetyRating[] SafetyRatings { get; set; }
    }

    internal sealed class Content
    {
        [JsonProperty("parts")]
        public Part[] Parts { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }
    }

    internal sealed class Part
    {
        [JsonProperty("text")]
        public string Text { get; set; }
    }

    internal sealed class PromptFeedback
    {
        [JsonProperty("safetyRatings")]
        public SafetyRating[] SafetyRatings { get; set; }
    }

    internal sealed class SafetyRating
    {
        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("probability")]
        public string Probability { get; set; }
    }
}
