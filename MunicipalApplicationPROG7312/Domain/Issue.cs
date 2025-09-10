using System;                                   // Guid, DateTime
using System.Collections.Generic;               // LinkedList<T>

namespace MunicipalApplicationPROG7312.Domain
{
    public enum IssueStatus { New, InProgress, Closed }   // Simple status enum

    public sealed class Issue
    {
        // Identity & audit
        public Guid Id { get; private set; }              // Ticket Id
        public DateTime CreatedAt { get; private set; }   // When user submitted

        // User-entered fields
        public string Location { get; private set; }      // Where the fault is
        public string Category { get; private set; }      // e.g., Sanitation, Roads
        public string Description { get; private set; }   // Free-text details

        // Attachments: use LinkedList (not List/array) to meet rubric
        public LinkedList<string> Attachments { get; } = new LinkedList<string>();

        // Workflow
        public IssueStatus Status { get; set; } = IssueStatus.New;

        // POPIA consent 
        public DateTime ConsentGivenAt { get; set; }              // Timestamp of consent
        public string ConsentTextVersion { get; set; } = "v1";    // Label of consent copy

        private Issue() { }                                       // Force factory use

        // Factory: accepts IEnumerable, stores in LinkedList
        public static Issue New(string location, string category, string description,
                                IEnumerable<string> attachments)
        {
            var issue = new Issue
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                Location = location,
                Category = category,
                Description = description
            };

            if (attachments != null)
            {
                foreach (var path in attachments)
                {
                    if (!string.IsNullOrWhiteSpace(path))
                        issue.Attachments.AddLast(path);
                }
            }

            return issue;
        }
    }
}
