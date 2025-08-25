using System;                                   // ArgumentException, DateTime
using System.Collections.Generic;               // IEnumerable
using MunicipalApplicationPROG7312.Persistance;
using MunicipalApplicationPROG7312.Persistence; // IssueStore (make sure namespace matches)

namespace MunicipalApplicationPROG7312.Domain
{
    public sealed class IssueService
    {
        private readonly IssueStore _store;                          // Composition over data store
        public IssueService(IssueStore store) { _store = store; }    // Inject store

        public Guid Submit(string location, string category, string description,
                           IEnumerable<string> attachments, string consentText)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(location)) throw new ArgumentException("location");
            if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("category");
            if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("description");

            // Build Issue from trimmed inputs; store attachments in LinkedList inside model
            var issue = Issue.New(location.Trim(), category.Trim(), description.Trim(), attachments);

            // Record POPIA consent details
            issue.ConsentGivenAt = DateTime.Now;                           // When consent was given
            issue.ConsentTextVersion = string.IsNullOrWhiteSpace(consentText) ? "v1" : consentText;

            // Persist into your in-memory store and return Id
            return _store.Add(issue);
        }
    }
}
