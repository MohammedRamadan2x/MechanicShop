using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Billing.Commands.SettleInvoice;

public sealed record SettleInvoiceCommand(Guid InvoiceId) : IRequest<Result<Success>>;