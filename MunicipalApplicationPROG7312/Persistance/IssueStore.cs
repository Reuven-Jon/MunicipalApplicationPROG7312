using System;
using System.Collections.Generic;

namespace MunicipalApplicationPROG7312.Persistence
{
    public sealed class IssueStore
    {
        private readonly LinkedList<MunicipalApplicationPROG7312.Domain.Issue> _issues =
            new LinkedList<MunicipalApplicationPROG7312.Domain.Issue>();

        private readonly Dictionary<Guid, LinkedListNode<MunicipalApplicationPROG7312.Domain.Issue>> _index =
            new Dictionary<Guid, LinkedListNode<MunicipalApplicationPROG7312.Domain.Issue>>();

        private readonly Queue<Guid> _pending = new Queue<Guid>();
        private readonly Stack<Guid> _recent = new Stack<Guid>();

        public static IssueStore Instance { get; } = new IssueStore();
        private IssueStore() { }

        public Guid Add(MunicipalApplicationPROG7312.Domain.Issue issue)
        {
            if (issue == null) throw new ArgumentNullException(nameof(issue));

            var node = _issues.AddLast(issue);
            _index[issue.Id] = node;
            _pending.Enqueue(issue.Id);
            _recent.Push(issue.Id);
            return issue.Id;
        }

        public bool TryGet(Guid id, out MunicipalApplicationPROG7312.Domain.Issue issue)
        {
            if (_index.TryGetValue(id, out var node))
            {
                issue = node.Value;
                return true;
            }
            issue = null;
            return false;
        }

        public IEnumerable<MunicipalApplicationPROG7312.Domain.Issue> All()
        {
            for (var n = _issues.First; n != null; n = n.Next)
                yield return n.Value;
        }
    }
}
