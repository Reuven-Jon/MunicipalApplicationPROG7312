using System;
using System.Collections.Generic;

namespace MunicipalApplicationPROG7312.Domain
{
    public enum EventCategory
    {
        Community,
        Safety,
        Utilities,
        Traffic,
        Education,
        Health,
        Recreation
    }

    public sealed class LocalEvent
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public EventCategory Category { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Location { get; set; } = string.Empty;
        public bool IsAnnouncement { get; set; } = false;   // short alerts / outages / closures
        public int Urgency { get; set; } = 5;               // 1 (highest)…10 (lowest)
        public HashSet<string> Tags { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    public sealed class UserAction
    {
        public DateTime At { get; init; } = DateTime.Now;
        public string Query { get; init; } = string.Empty;
        public int? ClickedEventId { get; init; }
        public EventCategory? FilterCategory { get; init; }
    }
}
