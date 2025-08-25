using System;

namespace MunicipalApplicationPROG7312.Models
{
    // Simple enum for theme
    public enum AppTheme { Light, Dark }     // Two theme choices

    [Serializable]                            // Allow JSON serialization
    public class AppSettings
    {
        public string LanguageCode { get; set; } = "en";  // "en","af","xh","zu"
        public AppTheme Theme { get; set; } = AppTheme.Light; // Default light
        public int BaseFontSize { get; set; } = 9;         // 8..16 point
    }
}
