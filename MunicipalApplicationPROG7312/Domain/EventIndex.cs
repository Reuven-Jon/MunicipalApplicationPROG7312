using System;
using System.Collections.Generic;
using System.Linq;

namespace MunicipalApplicationPROG7312.Domain
{
    /// <summary>
    /// Inverted index + sorted timeline + category sets.
    /// Uses Dictionary, SortedDictionary and HashSet as required by rubric.
    /// </summary>
    public sealed class EventIndex
    {
        private readonly Func<IEnumerable<LocalEvent>> _source;

        private readonly Dictionary<string, HashSet<int>> _tokenToIds =
            new Dictionary<string, HashSet<int>>(StringComparer.OrdinalIgnoreCase);

        private readonly SortedDictionary<DateTime, HashSet<int>> _byStart =
            new SortedDictionary<DateTime, HashSet<int>>();

        private readonly Dictionary<EventCategory, HashSet<int>> _byCategory =
            new Dictionary<EventCategory, HashSet<int>>();

        private readonly Dictionary<int, LocalEvent> _byId = new Dictionary<int, LocalEvent>();

        public EventIndex(Func<IEnumerable<LocalEvent>> source)
        {
            _source = source;
            Build();
        }

        public void Build()
        {
            _tokenToIds.Clear();
            _byStart.Clear();
            _byCategory.Clear();
            _byId.Clear();

            foreach (var e in _source())
            {
                _byId[e.Id] = e;

                foreach (var t in TokenizeEvent(e))
                {
                    if (!_tokenToIds.TryGetValue(t, out var set))
                        _tokenToIds[t] = set = new HashSet<int>();
                    set.Add(e.Id);
                }

                var day = e.Start.Date;
                if (!_byStart.TryGetValue(day, out var idsForDay))
                    _byStart[day] = idsForDay = new HashSet<int>();
                idsForDay.Add(e.Id);

                if (!_byCategory.TryGetValue(e.Category, out var idsForCat))
                    _byCategory[e.Category] = idsForCat = new HashSet<int>();
                idsForCat.Add(e.Id);
            }
        }

        public IEnumerable<LocalEvent> Search(string query, EventCategory? category = null, DateTime? onDate = null)
        {
            HashSet<int>? results = null;

            var tokens = Tokenize(query).ToList();
            if (tokens.Count == 0)
            {
                results = new HashSet<int>(_byId.Keys);
            }
            else
            {
                foreach (var t in tokens)
                {
                    if (_tokenToIds.TryGetValue(t, out var set))
                        results = results == null ? new HashSet<int>(set) : results.Intersect(set).ToHashSet();
                    else
                        return Enumerable.Empty<LocalEvent>(); // token not found -> empty
                }
            }

            if (category.HasValue && _byCategory.TryGetValue(category.Value, out var idsForCat))
                results = results!.Intersect(idsForCat).ToHashSet();

            if (onDate.HasValue)
            {
                var d = onDate.Value.Date;
                var idsForDay = _byStart.TryGetValue(d, out var s) ? s : new HashSet<int>();
                results = results!.Intersect(idsForDay).ToHashSet();
            }

            return results!.Select(id => _byId[id]).OrderBy(e => e.Start);
        }

        public IEnumerable<LocalEvent> Upcoming(int take = 20)
        {
            var now = DateTime.Now.Date;
            var ids = new HashSet<int>();
            foreach (var kv in _byStart.Where(kv => kv.Key >= now).Take(30))
                ids.UnionWith(kv.Value);
            return ids.Select(id => _byId[id]).OrderBy(e => e.Start).Take(take);
        }

        private static IEnumerable<string> TokenizeEvent(LocalEvent e)
        {
            foreach (var p in new[] { e.Title, e.Description, e.Location })
                foreach (var t in Tokenize(p)) yield return t;
            foreach (var t in e.Tags) yield return t;
            yield return e.Category.ToString();
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
