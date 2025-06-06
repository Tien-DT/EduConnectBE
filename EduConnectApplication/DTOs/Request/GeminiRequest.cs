using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EduConnect.Application.DTOs.Request
{
    public class GeminiRequest
    {
        [JsonProperty("contents")]
        public GeminiContent[] Contents { get; set; }

        [JsonProperty("generationConfig")]
        public GenerationConfig GenerationConfig { get; set; }

        [JsonProperty("safetySettings")]
        public SafetySetting[] SafetySettings { get; set; }
    }

    public class GeminiContent
    {
        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("parts")]
        public GeminiPart[] Parts { get; set; }
    }

    public class GeminiPart
    {
        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public class GenerationConfig
    {
        [JsonProperty("temperature")]
        public double Temperature { get; set; }

        [JsonProperty("topK")]
        public int TopK { get; set; }

        [JsonProperty("topP")]
        public double TopP { get; set; }

        [JsonProperty("maxOutputTokens")]
        public int MaxOutputTokens { get; set; }
    }

    public class SafetySetting
    {
        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("threshold")]
        public string Threshold { get; set; }
    }
}
