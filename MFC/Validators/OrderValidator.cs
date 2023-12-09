using FluentValidation;
using MFC.Models;

namespace MFC.Validators;

public class OrderValidator : AbstractValidator<Order>
{
    public OrderValidator()
    {
        RuleFor(order => order.Id).NotEmpty();
        RuleFor(order => order.LineItems).NotEmpty();
    }
}