using System;
using System.Collections.Generic;

namespace MunicipalApplicationPROG7312.Persistence
{
    /// <summary>
    /// In-memory store for issues.
    /// Data structures are chosen to show competency beyond lists/arrays:
    ///  - LinkedList keeps submission order.
    ///  - Dictionary gives O(1) lookup by Id.
    ///  - Queue tracks items waiting for action.
    ///  - Stack tracks most recent activity.
    /// Singleton scope keeps one source of truth during the app session.
    /// </summary>
    public sealed class IssueStore
    {
        // Maintains issues in the order they were submitted.
        private readonly LinkedList<MunicipalApplicationPROG7312.Domain.Issue> _issues =
            new LinkedList<MunicipalApplicationPROG7312.Domain.Issue>();

        // Fast index from Id → node inside the linked list (O(1) lookup, update, remove if needed).
        private readonly Dictionary<Guid, LinkedListNode<MunicipalApplicationPROG7312.Domain.Issue>> _index =
            new Dictionary<Guid, LinkedListNode<MunicipalApplicationPROG7312.Domain.Issue>>();

        // FIFO queue of issue Ids that are still pending (useful for future worker/processor).
        private readonly Queue<Guid> _pending = new Queue<Guid>();

        // LIFO stack of recently touched issue Ids (quick “recent activity” view).
        private readonly Stack<Guid> _recent = new Stack<Guid>();

        // Single shared instance for the desktop app lifetime.
        public static IssueStore Instance { get; } = new IssueStore();

        // Prevent direct construction; consumers use IssueStore.Instance.
        private IssueStore() { }

        /// <summary>
        /// Adds an issue to all structures and returns its Id.
        /// </summary>
        public Guid Add(MunicipalApplicationPROG7312.Domain.Issue issue)
        {
            if (issue == null) throw new ArgumentNullException(nameof(issue));

            // Append to the tail to preserve submission order.
            var node = _issues.AddLast(issue);

            // Index for constant-time lookup by Id.
            _index[issue.Id] = node;

            // Track processing order for a future dispatcher.
            _pending.Enqueue(issue.Id);

            // Track most recent action for quick “recent” views.
            _recent.Push(issue.Id);

            return issue.Id;
        }

        /// <summary>
        /// Tries to get an issue by Id without throwing if it does not exist.
        /// </summary>
        public bool TryGet(Guid id, out MunicipalApplicationPROG7312.Domain.Issue issue)
        {
            // Use the index to find the list node in O(1).
            if (_index.TryGetValue(id, out var node))
            {
                issue = node.Value;
                return true;
            }
            issue = null;
            return false;
        }

        /// <summary>
        /// Iterates all issues in submission order.
        /// </summary>
        public IEnumerable<MunicipalApplicationPROG7312.Domain.Issue> All()
        {
            // Walk the linked list from head to tail.
            for (var n = _issues.First; n != null; n = n.Next)
                yield return n.Value;
        }
    }
}
