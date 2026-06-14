using System;
using System.Collections.Generic;
using System.Text;

namespace Contracts.Requests
{
    // Requests/AuthorisePaymentRequest.cs
    public record AuthorisePaymentRequest(
        Guid OrderId,
        decimal Amount,
        string PaymentMethodToken);
}
