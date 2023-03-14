namespace AllegroFee.Models;

public class Promotion
{
    public PromotionType Type { get; set; }
    public decimal Amount { get; set; }
}
public enum PromotionType
{
    PercentageDiscount,
    FixedDiscount,
    FreeDelivery
}