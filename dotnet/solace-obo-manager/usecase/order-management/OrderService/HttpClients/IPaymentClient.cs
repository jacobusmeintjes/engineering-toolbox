using Contracts.Requests;
using Contracts.Responses;

namespace OrderService.HttpClients
{

    // HttpClients/IPaymentClient.cs
    public interface IPaymentClient
    {
        Task<AuthorisePaymentResponse> AuthoriseAsync(
            AuthorisePaymentRequest request, CancellationToken ct);

        Task VoidAsync(VoidPaymentRequest request, CancellationToken ct);
    }

    // HttpClients/PaymentClient.cs
    public class PaymentClient(HttpClient http) : IPaymentClient
    {
        public async Task<AuthorisePaymentResponse> AuthoriseAsync(
            AuthorisePaymentRequest request, CancellationToken ct)
        {
            var response = await http.PostAsJsonAsync("payments/authorise", request, ct);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<AuthorisePaymentResponse>(ct)
                ?? throw new InvalidOperationException("Empty response from Payment Service");
        }

        public async Task VoidAsync(VoidPaymentRequest request, CancellationToken ct)
        {
            var response = await http.PostAsJsonAsync("payments/void", request, ct);
            response.EnsureSuccessStatusCode();
        }
    }
}
