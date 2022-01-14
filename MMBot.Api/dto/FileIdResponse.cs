using System.Text.Json.Serialization;

namespace MMBot.Api.dto
{
    public class FileIdResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
    }
}
