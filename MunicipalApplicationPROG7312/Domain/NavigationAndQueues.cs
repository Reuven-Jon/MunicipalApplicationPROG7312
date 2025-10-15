using System;
using System.Collections.Generic;

namespace MunicipalApplicationPROG7312.Domain
{
    /// <summary>Simple stack used for back navigation.</summary>
    public sealed class NavigationHistory<T>
    {
        private readonly Stack<T> _stack = new Stack<T>();
        public void Push(T item) => _stack.Push(item);
        public bool TryPop(out T item)
        {
            if (_stack.Count > 0) { item = _stack.Pop(); return true; }
            item = default!;
            return false;
        }
        public int Count => _stack.Count;
    }

    /// <summary>FIFO queue for rotating tips/notices.</summary>
    public sealed class NotificationQueue
    {
        private readonly Queue<string> _q = new Queue<string>();
        public void Enqueue(string msg) => _q.Enqueue(msg);
        public bool TryDequeue(out string msg)
        {
            if (_q.Count > 0) { msg = _q.Dequeue(); return true; }
            msg = string.Empty; return false;
        }
        public int Count => _q.Count;
    }

    /// <summary>
    /// Min-priority queue implemented via SortedDictionary<int, Queue<T>>
    /// (compatible with .NET Framework and .NET).
    /// Lower priority value = higher priority.
    /// </summary>
    public sealed class UrgentAnnouncementQueue<T>
    {
        private readonly SortedDictionary<int, Queue<T>> _buckets = new SortedDictionary<int, Queue<T>>();

        public void Enqueue(T item, int priority)
        {
            if (!_buckets.TryGetValue(priority, out var q))
                _buckets[priority] = q = new Queue<T>();
            q.Enqueue(item);
        }

        public bool TryDequeue(out T item)
        {
            foreach (var kv in _buckets)
            {
                if (kv.Value.Count > 0)
                {
                    item = kv.Value.Dequeue();
                    if (kv.Value.Count == 0) _buckets.Remove(kv.Key);
                    return true;
                }
            }
            item = default!;
            return false;
        }

        public int Count
        {
            get
            {
                int c = 0;
                foreach (var kv in _buckets) c += kv.Value.Count;
                return c;
            }
        }
    }
}
