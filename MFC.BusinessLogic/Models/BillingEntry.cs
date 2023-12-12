using System.ComponentModel.DataAnnotations;

namespace MFC.Models;

public class BillingEntry
{
    [Required]
    public string Id { get; set; }
    public string OccurredAt { get; set; }
    public Type Type { get; set; }
    public Offer Offer { get; set; }
    public Value Value { get; set; }
    public Tax Tax { get; set; }
    public Balance Balance { get; set; }
    [Required]
    public Order Order { get; set; }
}

public class Type
{
    public string Id { get; set; }
    public string Name { get; set; }
}

public class Balance
{
    public string Amount { get; set; }
    public string Currency { get; set; }
}

public class Value
{
    public string Amount { get; set; }
    public string Currency { get; set; }
}