using Microsoft.EntityFrameworkCore;
using SwiftBite.PaymentService.Domain.Entities;
using SwiftBite.PaymentService.Domain.Interfaces;

namespace SwiftBite.PaymentService.Infrastructure.Persistence.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _db;

    public PaymentRepository(PaymentDbContext db)
        => _db = db;

    public async Task<Payment?> GetByIdAsync(
        Guid id, CancellationToken ct = default)
        => await _db.Payments
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<Payment?> GetByOrderIdAsync(
        Guid orderId, CancellationToken ct = default)
        => await _db.Payments
            .FirstOrDefaultAsync(
                p => p.OrderId == orderId, ct);

    public async Task<Payment?> GetByRazorpayOrderIdAsync(
        string razorpayOrderId,
        CancellationToken ct = default)
        => await _db.Payments
            .FirstOrDefaultAsync(
                p => p.RazorpayOrderId == razorpayOrderId,
                ct);

    public async Task<IEnumerable<Payment>> GetByCustomerIdAsync(
        string customerId, CancellationToken ct = default)
        => await _db.Payments
            .Where(p => p.CustomerId == customerId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

    public async Task AddAsync(
        Payment payment, CancellationToken ct = default)
        => await _db.Payments.AddAsync(payment, ct);

    public Task UpdateAsync(
        Payment payment, CancellationToken ct = default)
    {
        _db.Payments.Update(payment);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(
        CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}