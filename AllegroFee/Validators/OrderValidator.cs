using AllegroFee.Models;
using FluentValidation;

public class OrderValidator : AbstractValidator<Order>
{
    public OrderValidator()
    {
        RuleFor(order => order.Id).NotEmpty();
        RuleFor(order => order.LineItems).NotEmpty();
    }
}