using System.Text.Json.Serialization;

namespace MMBotGA.dto
{
    public class GetFileRequest
    {
        [JsonPropertyName("source")]
        public string Source { get; set; }
    }
}
