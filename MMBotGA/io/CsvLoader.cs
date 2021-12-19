using System;
using System.IO;
using System.Threading.Tasks;

namespace MMBotGA.io
{
    internal static class CsvLoader
    {
        public static async Task<string> LoadAsync(string path, bool reverse)
        {
            var lines = await File.ReadAllLinesAsync(path);
            if (reverse)
            {
                Array.Reverse(lines);
            }
            return $"[{string.Join(',', lines)}]";
        }
    }
}
