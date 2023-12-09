using Newtonsoft.Json;

namespace MFC.Models;

public class Order
{
    public string Id { get; set; }
    public string MessageToSeller { get; set; }
    public string Status { get; set; }
    public List<LineItem> LineItems { get; set; }
    public List<object> Surcharges { get; set; }
    public List<object> Discounts { get; set; }
    public Summary Summary { get; set; }
    public string UpdatedAt { get; set; }
    public string Revision { get; set; }
}

public class Amount
{
    [JsonProperty("amount")]
    public string AmountValue { get; set; }
    public string Currency { get; set; }
}
public class LineItem
{
    public string Id { get; set; }
    public Offer Offer { get; set; }
    public int Quantity { get; set; }
    public Amount OriginalPrice { get; set; }
    public Amount Price { get; set; }
    public object Reconciliation { get; set; }
    public List<object> SelectedAdditionalServices { get; set; }
    public string BoughtAt { get; set; }
}

public class Offer
{
    public string Id { get; set; }
    public string Name { get; set; }
    public object External { get; set; }
}

public class Summary
{
    public Amount TotalToPay { get; set; }
}

