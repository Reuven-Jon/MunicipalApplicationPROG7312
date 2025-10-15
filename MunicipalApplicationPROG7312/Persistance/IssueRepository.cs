using System;                                     // Environment
using System.Collections.Generic;                 // LinkedList<T>
using System.IO;                                  // File IO
using Newtonsoft.Json;                            // JSON (Newtonsoft)
using MunicipalApplicationPROG7312.Domain;

namespace MunicipalApplicationPROG7312.Persistance
{
    /// <summary>
    /// Simple file-backed repository that keeps issues in a LinkedList
    /// (to satisfy the rubric’s requirement to demonstrate non-List data structures).
    /// Order of insertion is preserved; enumeration yields oldest → newest.
    /// </summary>
    public static class IssueRepository
    {
        // In-memory store (insertion order preserved)
        private static readonly LinkedList<Issue> _issues = new LinkedList<Issue>();

        // %AppData%\MunicipalReporter\issues.json
        private static readonly string _dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MunicipalReporter", "issues.json");

        /// <summary>
        /// Enumerate all issues without exposing a mutable collection.
        /// </summary>
        public static IEnumerable<Issue> All
        {
            get { return _issues; }   // LinkedList<T> implements IEnumerable<T>
        }

        /// <summary>
        /// Add an issue and persist immediately.
        /// </summary>
        public static void Add(Issue issue)
        {
            if (issue == null) throw new ArgumentNullException(nameof(issue));
            _issues.AddLast(issue);   // append to tail to keep chronological order
            SaveToDisk();
        }

        /// <summary>
        /// Load issues from disk (if file exists). Corrupt files are ignored to keep the app usable.
        /// </summary>
        public static void LoadFromDisk()
        {
            try
            {
                var folder = Path.GetDirectoryName(_dbPath);
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                if (!File.Exists(_dbPath)) return;

                var json = File.ReadAllText(_dbPath);

                // Newtonsoft can deserialize directly into LinkedList<T>
                var loaded = JsonConvert.DeserializeObject<LinkedList<Issue>>(json);

                _issues.Clear();
                if (loaded != null)
                {
                    // Re-hydrate in the same order
                    for (var node = loaded.First; node != null; node = node.Next)
                        _issues.AddLast(node.Value);
                }
            }
            catch
            {
                // Ignore read/parse errors; app should still run even if the file is bad or locked.
            }
        }

        /// <summary>
        /// Write current linked list to JSON.
        /// </summary>
        private static void SaveToDisk()
        {
            var folder = Path.GetDirectoryName(_dbPath);
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            // Serialize the linked list directly
            var json = JsonConvert.SerializeObject(_issues, Formatting.Indented);
            File.WriteAllText(_dbPath, json);
        }
    }
}
