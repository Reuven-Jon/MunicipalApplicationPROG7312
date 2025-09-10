using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace MunicipalApplicationPROG7312.Persistance
{
    /// <summary>
    /// Lightweight, file-based telemetry.
    /// Writes one JSON object per line (JSON Lines) under %AppData%\MunicipalReporter.
    /// Safe to call from UI threads; file writes are locked.
    /// </summary>
    public static class Telemetry
    {
        // Serialises concurrent writers in this process.
        private static readonly object Gate = new object();

        // Resolve the full path for a given telemetry file name.
        private static string PathFor(string name)
        {
            var root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); // e.g. C:\Users\<you>\AppData\Roaming
            var dir = Path.Combine(root, "MunicipalReporter");                              // per-app folder
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);                     // create on first use
            return Path.Combine(dir, name);
        }

        /// <summary>
        /// Log a post-submit rating (1–5) with timestamp.
        /// </summary>
        public static void LogRating(Guid ticketId, int rating)
        {
            Append("ratings.jsonl", new { ticketId, rating, at = DateTime.Now });
        }

        /// <summary>
        /// Log the consent decision captured at submit.
        /// </summary>
        public static void LogConsent(Guid ticketId, bool consent)
        {
            Append("consent.jsonl", new { ticketId, consent, at = DateTime.Now });
        }

        /// <summary>
        /// Log an error with a simple origin tag (where) and message.
        /// </summary>
        public static void LogError(string where, string message)
        {
            Append("errors.jsonl", new { where, message, at = DateTime.Now });
        }

        // Append one JSON line to the chosen file. Creates the file if missing.
        private static void Append(string file, object payload)
        {
            lock (Gate)
            {
                var line = JsonConvert.SerializeObject(payload) + Environment.NewLine; // compact JSON + newline
                File.AppendAllText(PathFor(file), line);
            }
        }
    }
}
