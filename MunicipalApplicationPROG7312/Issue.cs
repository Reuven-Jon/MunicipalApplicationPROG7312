
    /// <summary>
    /// Domain record for a resident's reported issue.
    /// Notes:
    ///  - Attachments use LinkedList<string> to meet the data-structures requirement.
    ///  - Factory method normalises and sets defaults (Id, CreatedAt, Status).
    ///  - Consent fields are filled by IssueService at submit time.
    /// </summary>
    public sealed class Issue
    {
        // Identity and audit
        public Guid Id { get; set; }                      // assigned by factory
        public DateTime CreatedAt { get; set; }           // set at creation

        // User-supplied fields
        public string Location { get; set; }              // e.g., "Ward 12, Main Rd"
        public string Category { get; set; }              // e.g., "Roads"
        public string Description { get; set; }           // free text

        // Files are stored as full paths; LinkedList satisfies the rubric
        public LinkedList<string> Attachments { get; } = new LinkedList<string>();

        // Workflow
        public string Status { get; set; }                // e.g., "New", "Queued", "Closed"

        // POPIA consent capture (set by service when submitting)
        public DateTime ConsentGivenAt { get; set; }
        public string ConsentTextVersion { get; set; }

        /// <summary>
        /// Factory to create a new Issue with sane defaults.
        /// </summary>
        public static Issue New(string location, string category, string description, IEnumerable<string> attachments)
        {
            // Basic trimming to avoid storing whitespace noise.
            var issue = new Issue
            {
                Id = Guid.NewGuid(),
                Location = location?.Trim(),
                Category = category?.Trim(),
                Description = description?.Trim(),
                CreatedAt = DateTime.Now,
                Status = "New"
            };

            // Copy any provided attachment paths into the linked list.
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

