using MechanicShop.Domain.WorkOrders.Billing;

namespace MechanicShop.Application.Common.Interfaces;

public interface IInvoicePdfGenerator
{
    byte[] Generate(Invoice invoice);
}