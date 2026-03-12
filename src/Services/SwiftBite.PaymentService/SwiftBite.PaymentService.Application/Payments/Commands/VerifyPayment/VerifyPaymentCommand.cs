using MediatR;
using SwiftBite.PaymentService.Application.Payments.DTOs;

namespace SwiftBite.PaymentService.Application.Payments.Commands.VerifyPayment;

public record VerifyPaymentCommand(
    string RazorpayOrderId,
    string RazorpayPaymentId,
    string RazorpaySignature
) : IRequest<PaymentDto>;