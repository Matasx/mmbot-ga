using MMBot.Api.dto;

namespace MMBot.Api;

public interface IMMBotApi
{
    Task<Minfo> GetInfoAsync(string broker, string pair);
    Task<FileIdResponse> UploadAsync(string data);
    Task<string> GetFileAsync(GetFileRequest request);
    Task<FileIdResponse> GenerateTradesAsync(GenTradesRequest request);
    Task<IList<RunResponse>> RunAsync(RunRequest request);
}