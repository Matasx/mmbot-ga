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
        private static string _directory;
        private readonly CsvWriter _csv;

        private static string Directory 
        {
            get
            {
                if (!string.IsNullOrEmpty(_directory)) return _directory;

                var dir = Path.Combine("results", DateTimeProvider.Start.ToString("s").Replace(':', '.'));
                var directory = new DirectoryInfo(dir);
                if (!directory.Exists) directory.Create();
                _directory = directory.FullName;
                return _directory;
            }
        }

        public CsvWrapper(string name)
        {
            var filename = Sanitize($"results-{name}.csv");
            var fullPath = Path.Combine(Directory, filename);

            var writer = new StreamWriter(fullPath, false);
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

        private static string Sanitize(string fileName)
        {
            return Path.GetInvalidFileNameChars()
                .Aggregate(fileName, (current, ch) => current.Replace(ch.ToString(), "+"));
        }
    }
}