using System;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;

namespace MMBotGA.io
{
    internal class CsvWrapper<TMap, TRecord> : IDisposable where TMap : ClassMap
    {
        private readonly CsvWriter _csv;

        public CsvWrapper(string name)
        {
            var writer = new StreamWriter(Sanitize($"results-{name}-{DateTime.Now.ToString("s").Replace(':', '.')}.csv"), false);
            _csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            _csv.Context.RegisterClassMap<TMap>();
            _csv.WriteHeader<TRecord>();
            _csv.NextRecord();
            _csv.Flush();
        }

        public void WriteRecord(TRecord record)
        {
            _csv.WriteRecord(record);
            _csv.NextRecord();
            _csv.Flush();
        }

        public void Dispose()
        {
            _csv?.Dispose();
        }

        private string Sanitize(string fileName)
        {
            return Path.GetInvalidFileNameChars()
                .Aggregate(fileName, (current, ch) => current.Replace(ch.ToString(), "+"));
        }
    }
}