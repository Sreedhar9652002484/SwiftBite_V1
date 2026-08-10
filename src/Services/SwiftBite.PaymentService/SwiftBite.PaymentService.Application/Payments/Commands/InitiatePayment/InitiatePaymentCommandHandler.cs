using MediatR;
using SwiftBite.PaymentService.Application.Payments.DTOs;
using SwiftBite.PaymentService.Domain.Entities;
using SwiftBite.PaymentService.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace SwiftBite.PaymentService.Application.Payments.Commands.InitiatePayment;

public class InitiatePaymentCommandHandler
    : IRequestHandler<InitiatePaymentCommand, PaymentInitiatedDto>
{
    private readonly IPaymentRepository _repo;
    private readonly IRazorpayService _razorpay;
    private readonly IConfiguration _config;

    public InitiatePaymentCommandHandler(
        IPaymentRepository repo,
        IRazorpayService razorpay,
        IConfiguration config)
    {
        _repo = repo;
        _razorpay = razorpay;
        _config = config;
    }

    public async Task<PaymentInitiatedDto> Handle(
        InitiatePaymentCommand cmd, CancellationToken ct)
    {
        // ✅ Check if payment already exists for this order
        var existing = await _repo
            .GetByOrderIdAsync(cmd.OrderId, ct);

        if (existing != null &&
            existing.Status == Domain.Enums.PaymentStatus.Captured)
            throw new InvalidOperationException(
                "Order already paid.");

        // ✅ Create Razorpay order
        var razorpayOrder = await _razorpay.CreateOrderAsync(
            cmd.Amount, "INR",
            $"order_{cmd.OrderId}", ct);

        // ✅ Save payment record
        var payment = Payment.Create(
            cmd.OrderId, cmd.CustomerId,
            cmd.Amount, cmd.Method);

        payment.SetRazorpayOrderId(
            razorpayOrder.RazorpayOrderId);

        await _repo.AddAsync(payment, ct);
        await _repo.SaveChangesAsync(ct);

        // ✅ Return checkout details to Angular
        return new PaymentInitiatedDto
        {
            PaymentId = payment.Id,
            RazorpayOrderId = razorpayOrder.RazorpayOrderId,
            Amount = cmd.Amount,
            Currency = "INR",
            RazorpayKeyId = _config["Razorpay:KeyId"]!,
            CustomerName = cmd.CustomerName,
            CustomerEmail = cmd.CustomerEmail,
            CustomerPhone = cmd.CustomerPhone
        };
    }
}