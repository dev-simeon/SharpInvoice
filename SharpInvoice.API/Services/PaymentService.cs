using Microsoft.Extensions.Options;
using SharpInvoice.API.DTOs;
using SharpInvoice.API.Interfaces.Services;
using SharpInvoice.Core.Interfaces;
using Stripe;
using SharpInvoice.Infrastructure.Options;

namespace SharpInvoice.API.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly StripeSettings _stripeSettings;

    public PaymentService(IUnitOfWork unitOfWork, IOptions<StripeSettings> stripeSettings)
    {
        _unitOfWork = unitOfWork;
        _stripeSettings = stripeSettings.Value;
        StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
    }

    public async Task<CreatePaymentIntentResponse> CreatePaymentIntentAsync(CreatePaymentIntentRequest request)
    {
        var invoice = await _unitOfWork.Invoices.GetByIdAsync(request.InvoiceId);
        if (invoice == null)
        {
            throw new Exception("Invoice not found.");
        }

        var paymentIntentService = new PaymentIntentService();
        var paymentIntent = await paymentIntentService.CreateAsync(new PaymentIntentCreateOptions
        {
            Amount = (long)(invoice.Total * 100), // Convert to cents
            Currency = invoice.Currency.ToLower(),
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true,
            },
        });

        return new CreatePaymentIntentResponse
        {
            ClientSecret = paymentIntent.ClientSecret
        };
    }

    public async Task HandleSuccessfulPaymentAsync(string paymentIntentId)
    {
        var paymentIntentService = new PaymentIntentService();
        var paymentIntent = await paymentIntentService.GetAsync(paymentIntentId);

        if (paymentIntent == null)
        {
            throw new Exception("Payment intent not found.");
        }

        // This is a simplified lookup. A more robust implementation would use metadata.
        var invoice = (await _unitOfWork.Invoices.FindAsync(i => i.Total == (decimal)paymentIntent.Amount / 100 && i.Currency.ToLower() == paymentIntent.Currency)).FirstOrDefault();

        if (invoice == null)
        {
            // Can't find invoice, maybe log this for manual intervention
            return;
        }

        invoice.ApplyPayment(invoice.Total, Core.Enums.PaymentMethod.Stripe, paymentIntent.Id);
        await _unitOfWork.Invoices.UpdateAsync(invoice);
        await _unitOfWork.SaveChangesAsync();
    }
} 