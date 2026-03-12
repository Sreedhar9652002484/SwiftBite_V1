using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SwiftBite.PaymentService.Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;
using Razorpay.Api;
namespace SwiftBite.PaymentService.Infrastructure.Razorpay;
public class RazorpayService : IRazorpayService
{
    private readonly string _keyId;
    private readonly string _keySecret;
    private readonly ILogger<RazorpayService> _logger;
    private readonly bool _isDevelopment;

    public RazorpayService(
        IConfiguration configuration,
        ILogger<RazorpayService> logger)
    {
        _keyId = configuration["Razorpay:KeyId"]!;
        _keySecret = configuration["Razorpay:KeySecret"]!;
        _logger = logger;
        _isDevelopment = _keyId.Contains("dummy");
    }

    public async Task<RazorpayOrderResult> CreateOrderAsync(
        decimal amount, string currency,
        string receipt, CancellationToken ct = default)
    {
        // ✅ Mock mode — no real Razorpay needed!
        if (_isDevelopment)
        {
            var mockOrderId = $"order_mock_{Guid.NewGuid():N}";
            _logger.LogInformation(
                "🧪 MOCK Razorpay order | Id: {Id} | ₹{Amount}",
                mockOrderId, amount);

            return new RazorpayOrderResult(
                mockOrderId, amount, currency, "created");
        }

        // 🔴 Real Razorpay — production only
        try
        {
            var client = new RazorpayClient(_keyId, _keySecret);

            var options = new Dictionary<string, object>
            {
                { "amount",          (int)(amount * 100) },
                { "currency",        currency },
                { "receipt",         receipt },
                { "payment_capture", 1 }
            };

            var order = client.Order.Create(options);
            string id = order["id"].ToString()!;
            string stat = order["status"].ToString()!;

            _logger.LogInformation(
                "💳 Razorpay order | Id: {Id} | ₹{Amount}",
                id, amount);

            return new RazorpayOrderResult(
                id, amount, currency, stat);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "❌ Razorpay order creation failed");
            throw;
        }
    }

    public bool VerifySignature(
        string razorpayOrderId,
        string razorpayPaymentId,
        string razorpaySignature)
    {
        // ✅ Mock mode — always return true in dev
        if (_isDevelopment)
        {
            _logger.LogInformation(
                "🧪 MOCK signature verification — VALID");
            return true;
        }

        // 🔴 Real verification
        try
        {
            var payload =
                $"{razorpayOrderId}|{razorpayPaymentId}";

            using var hmac = new HMACSHA256(
                Encoding.UTF8.GetBytes(_keySecret));

            var hash = hmac.ComputeHash(
                Encoding.UTF8.GetBytes(payload));

            var generated = BitConverter
                .ToString(hash)
                .Replace("-", "")
                .ToLower();

            var isValid = generated == razorpaySignature;

            _logger.LogInformation(
                "🔐 Signature: {Result}",
                isValid ? "VALID ✅" : "INVALID ❌");

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Signature error");
            return false;
        }
    }

    public async Task<string> ProcessRefundAsync(
        string razorpayPaymentId,
        decimal amount,
        CancellationToken ct = default)
    {
        // ✅ Mock refund
        if (_isDevelopment)
        {
            var mockRefundId = $"rfnd_mock_{Guid.NewGuid():N}";
            _logger.LogInformation(
                "🧪 MOCK refund | Id: {Id} | ₹{Amount}",
                mockRefundId, amount);
            return mockRefundId;
        }

        // 🔴 Real refund
        try
        {
            var client = new RazorpayClient(_keyId, _keySecret);

            var payment = client.Payment
                .Fetch(razorpayPaymentId);

            var refund = payment.Refund(
                new Dictionary<string, object>
                {
                    { "amount", (int)(amount * 100) }
                });

            string refundId = refund["id"].ToString()!;

            _logger.LogInformation(
                "💰 Refund | Id: {Id} | ₹{Amount}",
                refundId, amount);

            return refundId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Refund failed");
            throw;
        }
    }
}