using System;
using System.Collections.Generic;
using MunicipalApplicationPROG7312.Domain;

namespace MunicipalApplicationPROG7312.Persistance
{
    /// <summary>
    /// Simple in-memory data source; you can swap this later for JSON/DB
    /// without changing UI by keeping the IEventStore interface.
    /// </summary>
    public sealed class InMemoryEventStore : IEventStore
    {
        private readonly Dictionary<int, LocalEvent> _events = new Dictionary<int, LocalEvent>();
        private int _nextId = 1;

        public InMemoryEventStore()
        {
            Seed();
        }

        public IEnumerable<LocalEvent> All() => _events.Values;
        public LocalEvent? GetById(int id) => _events.TryGetValue(id, out var e) ? e : null;

        public void Add(LocalEvent e)
        {
            e.Id = _nextId++;
            _events[e.Id] = e;
        }

        private void Seed()
        {
            Add(new LocalEvent
            {
                Title = "Planned Water Outage - Ward 61",
                Description = "Maintenance on main line. Affected streets listed on city site.",
                Category = EventCategory.Utilities,
                Start = DateTime.Today.AddDays(1).AddHours(9),
                End = DateTime.Today.AddDays(1).AddHours(16),
                Location = "Ward 61",
                IsAnnouncement = true,
                Urgency = 2,
                Tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "water", "maintenance", "outage", "ward61" }
            });

            Add(new LocalEvent
            {
                Title = "Road Closure: Main Rd 09:00–13:00",
                Description = "Parade route via Church St. Use alternative routes.",
                Category = EventCategory.Traffic,
                Start = DateTime.Today.AddDays(2).AddHours(9),
                End = DateTime.Today.AddDays(2).AddHours(13),
                Location = "Main Rd",
                IsAnnouncement = true,
                Urgency = 1,
                Tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "traffic", "closure", "parade" }
            });

            Add(new LocalEvent
            {
                Title = "Community Clean-up: Strandfontein Dunes",
                Description = "Bring gloves. Bags provided. Families welcome.",
                Category = EventCategory.Community,
                Start = DateTime.Today.AddDays(3).AddHours(8),
                End = DateTime.Today.AddDays(3).AddHours(11),
                Location = "Strandfontein Pavilion",
                Tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "cleanup", "beach", "volunteer" }
            });

            Add(new LocalEvent
            {
                Title = "Health Screening Bus",
                Description = "Free BP and glucose testing.",
                Category = EventCategory.Health,
                Start = DateTime.Today.AddDays(5).AddHours(10),
                End = DateTime.Today.AddDays(5).AddHours(14),
                Location = "Pelican Park Library",
                Tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "clinic", "free", "wellness" }
            });
        }
    }
}
