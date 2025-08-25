public class Issue
{
    public Guid Id { get; set; }
    public string Location { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
    public List<string> Attachments { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; }
    public DateTime ConsentGivenAt { get; set; } // Add this property to fix CS1061
    public string ConsentTextVersion { get; set; } // Add this property if not already present

    public static Issue New(string location, string category, string description, List<string> attachments)
    {
        return new Issue
        {
            Id = Guid.NewGuid(),
            Location = location,
            Category = category,
            Description = description,
            Attachments = attachments ?? new List<string>(),
            CreatedAt = DateTime.Now,
            Status = "New"
        };
    }
}
