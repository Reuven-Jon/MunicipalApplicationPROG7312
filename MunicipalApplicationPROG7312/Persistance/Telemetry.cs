using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace MunicipalApplicationPROG7312.Persistance
{
    public static class Telemetry
    {
        private static readonly object Gate = new object();
        private static string PathFor(string name)
        {
            var root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = Path.Combine(root, "MunicipalReporter");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return Path.Combine(dir, name);
        }

        public static void LogRating(Guid ticketId, int rating)
        {
            Append("ratings.jsonl", new { ticketId, rating, at = DateTime.Now });
        }

        public static void LogConsent(Guid ticketId, bool consent)
        {
            Append("consent.jsonl", new { ticketId, consent, at = DateTime.Now });
        }

        public static void LogError(string where, string message)
        {
            Append("errors.jsonl", new { where, message, at = DateTime.Now });
        }

        private static void Append(string file, object payload)
        {
            lock (Gate)
            {
                File.AppendAllText(PathFor(file), JsonConvert.SerializeObject(payload) + Environment.NewLine);
            }
        }
    }
}
