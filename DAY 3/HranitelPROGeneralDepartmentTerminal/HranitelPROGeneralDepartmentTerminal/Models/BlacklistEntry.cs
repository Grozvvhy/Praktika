using System;

public class BlacklistEntry
{
    public int Id { get; set; }
    public int VisitorId { get; set; }
    public string Reason { get; set; }
    public DateTime AddedAt { get; set; }
}