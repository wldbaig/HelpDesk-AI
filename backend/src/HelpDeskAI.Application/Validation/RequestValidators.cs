using FluentValidation;
using HelpDeskAI.Application.DTOs;
using HelpDeskAI.Domain.Enums;

namespace HelpDeskAI.Application.Validation;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).MinimumLength(8).Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.").Matches("[0-9]").WithMessage("Password must contain a number.");
    }
}

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class CreateTicketRequestValidator : AbstractValidator<CreateTicketRequest>
{
    public CreateTicketRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(5000);
        RuleFor(x => x.Priority).Must(BePriority).WithMessage("Priority must be Low, Medium, High, or Urgent.");
    }
    private static bool BePriority(string value) => Enum.TryParse<TicketPriority>(value, true, out _);
}

public sealed class UpdateTicketRequestValidator : AbstractValidator<UpdateTicketRequest>
{
    public UpdateTicketRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(5000);
        RuleFor(x => x.Category).IsEnumName(typeof(TicketCategory), false);
        RuleFor(x => x.Priority).IsEnumName(typeof(TicketPriority), false);
        RuleFor(x => x.Status).IsEnumName(typeof(TicketStatus), false);
        RuleFor(x => x.SuggestedReply).MaximumLength(5000);
    }
}

public sealed class AddCommentRequestValidator : AbstractValidator<AddCommentRequest>
{
    public AddCommentRequestValidator() => RuleFor(x => x.Body).NotEmpty().MaximumLength(2000);
}
