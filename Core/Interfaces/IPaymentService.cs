using Stripe;

namespace Core.Interfaces
{
    public interface IPaymentService
    {
        public Task<PaymentIntent> CreateOrUpdatePaymentIntent(string userId);
    }
}
