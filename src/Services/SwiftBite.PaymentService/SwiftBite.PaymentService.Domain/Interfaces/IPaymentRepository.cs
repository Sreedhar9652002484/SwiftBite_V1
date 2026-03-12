using SwiftBite.PaymentService.Domain.Entities;

namespace SwiftBite.PaymentService.Domain.Interfaces;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id,
        CancellationToken ct = default);

    Task<Payment?> GetByOrderIdAsync(Guid orderId,
        CancellationToken ct = default);

    Task<Payment?> GetByRazorpayOrderIdAsync(
        string razorpayOrderId,
        CancellationToken ct = default);

    Task<IEnumerable<Payment>> GetByCustomerIdAsync(
        string customerId,
        CancellationToken ct = default);

    Task AddAsync(Payment payment,
        CancellationToken ct = default);

    Task UpdateAsync(Payment payment,
        CancellationToken ct = default);

    Task SaveChangesAsync(
        CancellationToken ct = default);
}