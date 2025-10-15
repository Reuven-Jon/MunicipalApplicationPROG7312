using System;
using System.Collections.Generic;
using System.Linq;

namespace MunicipalApplicationPROG7312.Domain
{
    /// <summary>
    /// Scores events by token overlap, category affinity, recency, announcement bonus.
    /// </summary>
    public sealed class RecommendationEngine
    {
        private readonly Func<IEnumerable<LocalEvent>> _events;
        private readonly EventIndex _index;
        private readonly List<UserAction> _actions = new List<UserAction>();

        public RecommendationEngine(Func<IEnumerable<LocalEvent>> eventsSource, EventIndex index)
        {
            _events = eventsSource;
            _index = index;
        }

        public void Track(UserAction action) => _actions.Add(action);

        public IEnumerable<LocalEvent> RecommendTopN(int n = 5)
        {
            if (_actions.Count == 0) return _index.Upcoming(n);

            var tokens = _actions.SelectMany(a => Tokenize(a.Query)).ToList();
            var favCat = _actions.Where(a => a.FilterCategory.HasValue)
                                 .GroupBy(a => a.FilterCategory!.Value)
                                 .OrderByDescending(g => g.Count())
                                 .Select(g => (EventCategory?)g.Key)
                                 .FirstOrDefault();

            var scored = new List<(LocalEvent e, double score)>();
            foreach (var e in _events())
            {
                double score = 0;

                if (tokens.Count > 0)
                {
                    var eventTokens = new HashSet<string>(Tokenize($"{e.Title} {e.Description} {e.Location}"));
                    int overlap = tokens.Count(t => eventTokens.Contains(t));
                    score += overlap * 2.0;
                }

                if (favCat.HasValue && e.Category == favCat.Value) score += 3.0;

                var days = (e.Start - DateTime.Now).TotalDays;
                if (days >= 0 && days <= 7) score += (7 - days); // recency 0..7

                if (e.IsAnnouncement) score += 1.5;

                scored.Add((e, score));
            }

            return scored.OrderByDescending(s => s.score).ThenBy(s => s.e.Start).Take(n).Select(s => s.e);
        }

        private static IEnumerable<string> Tokenize(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) yield break;
            var parts = text!.ToLowerInvariant()
                .Replace(":", " ").Replace(",", " ").Replace(".", " ")
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in parts) yield return p.Trim();
        }
    }
}
