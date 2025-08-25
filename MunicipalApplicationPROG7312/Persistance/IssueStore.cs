using System;
using System.Collections.Generic;
using MunicipalApplicationPROG7312.Domain;

namespace MunicipalApplicationPROG7312.Persistence
{
    public sealed class IssueStore
    {
        private readonly LinkedList<Issue> _issues = new LinkedList<Issue>();                 // Ordered tickets
        private readonly Dictionary<Guid, LinkedListNode<Issue>> _index = new Dictionary<Guid, LinkedListNode<Issue>>(); // O(1) by Id
        private readonly Queue<Guid> _pending = new Queue<Guid>();                            // FIFO for next actions
        private readonly Stack<Guid> _recent = new Stack<Guid>();                             // LIFO for recent history

        public Guid Add(Issue issue)
        {
            var node = _issues.AddLast(issue);   // Keep submission order
            _index[issue.Id] = node;             // Index for quick lookup
            _pending.Enqueue(issue.Id);          // Queue for processing
            _recent.Push(issue.Id);              // Track recent
            return issue.Id;
        }

        public bool TryGet(Guid id, out Issue issue)
        {
            if (_index.TryGetValue(id, out var node)) { issue = node.Value; return true; }
            issue = null; return false;
        }

        public IEnumerable<Issue> All()
        {
            for (var n = _issues.First; n != null; n = n.Next) yield return n.Value;
        }
    }
}
