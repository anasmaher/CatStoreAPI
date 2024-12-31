using Castle.Core.Configuration;
using Core.Interfaces;
using Stripe;
using Stripe.Checkout;

public class PaymentService : IPaymentService
{
    private readonly IShoppingCartRepository cartRepository;

    public PaymentService(IShoppingCartRepository cartRepository)
    {
        this.cartRepository = cartRepository;

        StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("SecretKey");
    }

    public async Task<PaymentIntent> CreateOrUpdatePaymentIntent(string userId)
    {
        var cart = await cartRepository.GetCartWithItemsAsync(userId);
        if (cart == null || !cart.Items.Any())
            return null;

        var paymentIntentService = new PaymentIntentService();
        var paymentIntentOptions = new PaymentIntentCreateOptions
        {
            Amount = (long)(cart.price * 100),
            Currency = "usd",
            PaymentMethodTypes = new List<string> { "card" },
        };

        var paymentIntent = await paymentIntentService.CreateAsync(paymentIntentOptions);
        return paymentIntent;
    }
}