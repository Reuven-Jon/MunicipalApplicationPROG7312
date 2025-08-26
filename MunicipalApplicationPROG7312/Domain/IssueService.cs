using System;
using System.Collections.Generic;
using MunicipalApplicationPROG7312.Persistence; 

namespace MunicipalApplicationPROG7312.Domain
{
    public sealed class IssueService
    {
        private readonly IssueStore _store;
        public IssueService(IssueStore store) { _store = store; }

        public Guid Submit(string location, string category, string description,
                           IEnumerable<string> attachments, string consentText)
        {
            if (string.IsNullOrWhiteSpace(location)) throw new ArgumentException("location");
            if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("category");
            if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("description");

            var issue = Issue.New(location.Trim(), category.Trim(), description.Trim(), attachments);
            issue.ConsentGivenAt = DateTime.Now;
            issue.ConsentTextVersion = string.IsNullOrWhiteSpace(consentText) ? "v1" : consentText.Trim();

            return _store.Add(issue);   // hits the implemented method above
        }
    }
}
