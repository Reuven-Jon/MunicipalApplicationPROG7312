using System;
using DomIssue = MunicipalApplicationPROG7312.Domain.Issue;

namespace MunicipalApplicationPROG7312.Persistence
{
    public static class EventHub
    {
        public static event Action<DomIssue>? IssueReported;
        public static void PublishIssueReported(DomIssue issue) => IssueReported?.Invoke(issue);
    }
}
