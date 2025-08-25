using System;                                    // Environment
using System.Collections.Generic;                // List<T>
using System.IO;                                 // File IO
using Newtonsoft.Json;                           // JSON (Newtonsoft)
using MunicipalApplicationPROG7312.Domain;

namespace MunicipalApplicationPROG7312.Persistance
{
    public static class IssueRepository
    {
        private static readonly List<Issue> _issues = new List<Issue>();  // in-memory cache
        private static readonly string _dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MunicipalReporter", "issues.json");

        public static IReadOnlyList<Issue> All { get { return _issues.AsReadOnly(); } }

        public static void Add(Issue issue)
        {
            _issues.Add(issue);             // keep in memory
            SaveToDisk();                   // persist to JSON
        }

        public static void LoadFromDisk()
        {
            try
            {
                var folder = Path.GetDirectoryName(_dbPath);
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                if (!File.Exists(_dbPath)) return;

                var json = File.ReadAllText(_dbPath);                  // read file
                var loaded = JsonConvert.DeserializeObject<List<Issue>>(json);
                _issues.Clear();
                if (loaded != null) _issues.AddRange(loaded);          // hydrate cache
            }
            catch
            {
                // Ignore corrupt/locked file to keep app running
            }
        }

        private static void SaveToDisk()
        {
            var folder = Path.GetDirectoryName(_dbPath);
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            var json = JsonConvert.SerializeObject(_issues, Formatting.Indented);
            File.WriteAllText(_dbPath, json);                          // write file
        }
    }
}
