namespace RavenPost.Api.Models;

public class DispatchItem
{
    public int Id { get; set; }

    public int DispatchId { get; set; }
    public Dispatch Dispatch { get; set; } = null!;

    public int SupplyId { get; set; }
    public Supply Supply { get; set; } = null!;

    public int Quantity { get; set; }
    public decimal LineCost { get; set; }
}
