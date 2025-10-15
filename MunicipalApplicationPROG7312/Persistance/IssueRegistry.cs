using System;
using System.Collections.Generic;
using DomIssue = MunicipalApplicationPROG7312.Domain.Issue;

namespace MunicipalApplicationPROG7312.Persistence
{
    public sealed class IssueRegistry
    {
        private static readonly Lazy<IssueRegistry> _lazy = new(() => new IssueRegistry());
        public static IssueRegistry Instance => _lazy.Value;

        private IssueRegistry() { }

        public Dictionary<Guid, DomIssue> Index { get; } = new();
        public Queue<Guid> Pending { get; } = new();
        public Stack<Guid> RecentlyReported { get; } = new();
    }
}
