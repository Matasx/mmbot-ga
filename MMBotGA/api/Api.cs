using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MMBotGA.dto;

namespace MMBotGA.api
{
    internal class Api
    {
        private readonly string _baseUrl;
        private readonly HttpClient _client;

        private readonly JsonSerializerOptions _serializerOptions = new ()
        {
            Converters = { new DoubleConverter() },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private string BackestUrl => _baseUrl + "backtest2/";
        private string BrokersUrl => _baseUrl + "brokers/";

        public Api(string baseUrl, HttpClient client)
        {
            _baseUrl = baseUrl;
            _client = client;
        }

        public async Task<Minfo> GetInfoAsync(string broker, string pair)
        {
            using var response = await _client.GetAsync($"{BrokersUrl}{broker}/pairs/{pair}/info");
            
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<Minfo>(result, _serializerOptions);
        }

        public async Task<FileIdResponse> UploadAsync(string data)
        {
            using var response = await _client.PostAsync(BackestUrl + "upload", new StringContent(data));

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<FileIdResponse>(result, _serializerOptions);
        }

        public async Task<string> GetFileAsync(GetFileRequest request)
        {
            var content = JsonSerializer.Serialize(request, _serializerOptions);

            using var response = await _client.PostAsync(BackestUrl + "get_file", new StringContent(content));

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<FileIdResponse> GenerateTradesAsync(GenTradesRequest request)
        {
            var content = JsonSerializer.Serialize(request, _serializerOptions);

            using var response = await _client.PostAsync(BackestUrl + "gen_trades", new StringContent(content));

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<FileIdResponse>(result, _serializerOptions);
        }

        public async Task<IList<RunResponse>> RunAsync(RunRequest request)
        {
            var content = JsonSerializer.Serialize(request, _serializerOptions);

            using var response = await _client.PostAsync(BackestUrl + "run", new StringContent(content));

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<IList<RunResponse>>(result, _serializerOptions);
        }

        private class DoubleConverter : JsonConverter<double>
        {
            public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return reader.TokenType == JsonTokenType.Number ? reader.GetDouble() : double.NaN;
            }

            public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
            {
                writer.WriteNumberValue(value);
            }
        }
    }
}
