using System.Text.Json.Serialization;

namespace MMBotGA.dto
{
    public class FileIdResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
    }
}
