using SharpInvoice.API.Dtos;

namespace SharpInvoice.API.Interfaces.Services;

public interface IPaymentService
{
    Task<CreatePaymentIntentResponse> CreatePaymentIntentAsync(CreatePaymentIntentRequest request);
    Task HandleSuccessfulPaymentAsync(string paymentIntentId);
} 