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
        public GeminiContent[] Contents { get; set; } = Array.Empty<GeminiContent>();

        [JsonProperty("generationConfig")]
        public GenerationConfig? GenerationConfig { get; set; }

        [JsonProperty("safetySettings")]
        public SafetySetting[]? SafetySettings { get; set; }

        [JsonProperty("tools")]
        public GeminiTool[]? Tools { get; set; }

        [JsonProperty("toolConfig")]
        public ToolConfig? ToolConfig { get; set; }
    }

    public class GeminiContent
    {
        [JsonProperty("role")]
        public string Role { get; set; } = string.Empty;

        [JsonProperty("parts")]
        public GeminiPart[] Parts { get; set; } = Array.Empty<GeminiPart>();
    }

    public class GeminiPart
    {
        [JsonProperty("text")]
        public string? Text { get; set; }

        [JsonProperty("functionCall")]
        public FunctionCall? FunctionCall { get; set; }

        [JsonProperty("functionResponse")]
        public FunctionResponse? FunctionResponse { get; set; }
    }

    public class FunctionCall
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("args")]
        public Dictionary<string, object> Args { get; set; } = new();
    }

    public class FunctionResponse
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("response")]
        public Dictionary<string, object> Response { get; set; } = new();
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
        public string Category { get; set; } = string.Empty;

        [JsonProperty("threshold")]
        public string Threshold { get; set; } = string.Empty;
    }

    public class GeminiTool
    {
        [JsonProperty("functionDeclarations")]
        public FunctionDeclaration[] FunctionDeclarations { get; set; } = Array.Empty<FunctionDeclaration>();
    }

    public class FunctionDeclaration
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("parameters")]
        public FunctionParameters Parameters { get; set; } = new();
    }

    public class FunctionParameters
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "object";

        [JsonProperty("properties")]
        public Dictionary<string, ParameterSchema> Properties { get; set; } = new();

        [JsonProperty("required")]
        public string[] Required { get; set; } = Array.Empty<string>();
    }

    public class ParameterSchema
    {
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("enum")]
        public string[]? Enum { get; set; }
    }

    public class ToolConfig
    {
        [JsonProperty("functionCallingConfig")]
        public FunctionCallingConfig FunctionCallingConfig { get; set; } = new();
    }

    public class FunctionCallingConfig
    {
        [JsonProperty("mode")]
        public string Mode { get; set; } = "AUTO";

        [JsonProperty("allowedFunctionNames")]
        public string[]? AllowedFunctionNames { get; set; }
    }
}
