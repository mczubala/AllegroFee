using System.ComponentModel.DataAnnotations;

namespace MFC.Models;

public class BillingEntry
{
    [Required]
    public string Id { get; set; }
    public DateTime OccurredAt { get; set; }
    public TypeData Type { get; set; }
    public OfferData Offer { get; set; }
    public MoneyData Value { get; set; }
    public TaxData Tax { get; set; }
    public MoneyData Balance { get; set; }
    [Required]
    public OrderData Order { get; set; }
}

public class TypeData
{
    public string Id { get; set; }
    public string Name { get; set; }
}

public class OfferData
{
    public string Id { get; set; }
    public string Name { get; set; }
}

public class MoneyData
{
    public string Amount { get; set; }
    public string Currency { get; set; }
}

public class TaxData
{
    public string Percentage { get; set; }
}

public class OrderData
{
    public string Id { get; set; }
}
