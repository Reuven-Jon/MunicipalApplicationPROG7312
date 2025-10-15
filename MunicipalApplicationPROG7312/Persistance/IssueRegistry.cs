using System;
using System.Collections.Generic;
using MunicipalApplicationPROG7312.Domain;

namespace MunicipalApplicationPROG7312.Persistence
{
    /// <summary>
    /// Central, in-memory registries in orderr to satisfy the rubric:
    /// - Dictionary: O(1) lookup by Id
    /// - Queue: FIFO of "pending" issues awaiting processing
    /// - Stack: LIFO of "recently reported" issues this session
    /// </summary>
    public sealed class IssueRegistry
    {
        private static readonly Lazy<IssueRegistry> _lazy = new(() => new IssueRegistry());
        public static IssueRegistry Instance => _lazy.Value;

        private IssueRegistry() { }

        public Dictionary<Guid, Issue> Index { get; } = new();     // Id -> Issue
        public Queue<Guid> Pending { get; } = new();                // Pending issue Ids
        public Stack<Guid> RecentlyReported { get; } = new();       // Last reported Ids (session)
    }
}
