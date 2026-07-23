namespace CqrsExample.Entities;

public class Outbox
{
    public int Id { get; set; }
    public string EventType { get; set; } = null!;
    public string Payload { get; set; } = null!;
    public bool IsProcessed { get; set; }
    public bool? Success { get; set; }
    public int ProcessAttempts { get; set; }
    public DateTime CreatedAt { get; set; }
}