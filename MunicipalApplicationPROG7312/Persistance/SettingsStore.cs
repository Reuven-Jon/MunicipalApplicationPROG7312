using System;                                  // EventHandler
using System.IO;                               // File IO
using Newtonsoft.Json;                         // JSON
using MunicipalApplicationPROG7312.Models;             // AppSettings

namespace MunicipalApplicationPROG7312.Persistence
{
    // Central settings loader/saver with change event
    public static class SettingsStore
    {
        private static readonly object Gate = new object(); // Thread gate

        public static AppSettings Current { get; private set; } = new AppSettings(); // Live settings

        // Raised when settings change so forms can refresh
        public static event EventHandler SettingsChanged;

        private static string Path()
        {
            var root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);        // %AppData%
            var dir = System.IO.Path.Combine(root, "MunicipalReporter");                            // App folder
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);                             // Create if needed
            return System.IO.Path.Combine(dir, "settings.json");                                    // File path
        }

        public static void Load()
        {
            lock (Gate)
            {
                var p = Path();                                                                     // Resolve path
                if (!File.Exists(p)) { Save(); return; }                                            // Create defaults
                try
                {
                    var json = File.ReadAllText(p);                                                 // Read file
                    var s = JsonConvert.DeserializeObject<AppSettings>(json);                       // Parse JSON
                    if (s != null) Current = s;                                                     // Adopt if valid
                }
                catch { /* keep defaults on error */ }                                              // Safe fallback
            }
        }

        public static void Save()
        {
            lock (Gate)
            {
                var p = Path();                                                                     // Resolve path
                var json = JsonConvert.SerializeObject(Current, Formatting.Indented);               // To JSON
                File.WriteAllText(p, json);                                                         // Persist
            }
        }

        public static void Update(AppSettings next)
        {
            lock (Gate) { Current = next; Save(); }                                                 // Replace + write
            var handler = SettingsChanged;                                                          // Copy delegate
            if (handler != null) handler(null, EventArgs.Empty);                                    // Notify listeners
        }
    }
}
