using System.Text.Json.Serialization;

namespace MMBot.Api.dto
{
    public class GetFileRequest
    {
        [JsonPropertyName("source")]
        public string Source { get; set; }
    }
}
