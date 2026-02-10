namespace RavenPost.Api.Models;

public class Dispatch
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal TotalCost { get; set; }
    public List<DispatchItem> Items { get; set; } = new();
}
