using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace MFC.Models;

public class Order
{
    [Required]
    public string Id { get; set; }
    public string MessageToSeller { get; set; }
    public string Status { get; set; }
    [Required]
    public List<LineItem> LineItems { get; set; }
    public List<object> Surcharges { get; set; }
    public List<object> Discounts { get; set; }
    public Summary Summary { get; set; }
    public string UpdatedAt { get; set; }
    public string Revision { get; set; }
}

public class LineItem
{
    [Required]
    public string Id { get; set; }
    public Offer Offer { get; set; }
    public int Quantity { get; set; }
    public OriginalPrice OriginalPrice { get; set; }
    public Price Price { get; set; }
    public object Reconciliation { get; set; }
    public List<object> SelectedAdditionalServices { get; set; }
    public string BoughtAt { get; set; }
}

public class Summary
{
    public TotalToPay TotalToPay { get; set; }
}
public class TotalToPay
{
    public decimal Amount { get; set; }
    public string Currency { get; set; }
}
