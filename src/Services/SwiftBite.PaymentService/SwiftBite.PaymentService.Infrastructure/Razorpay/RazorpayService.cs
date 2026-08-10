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

    // ✅ Generate safe receipt (ALWAYS < 40 chars)
    private string GenerateReceipt()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = Guid.NewGuid().ToString("N")[..4]; // short suffix
        return $"ord_{timestamp}_{random}";
    }

    public async Task<RazorpayOrderResult> CreateOrderAsync(
        decimal amount,
        string currency,
        string receipt,
        CancellationToken ct = default)
    {
        // ✅ Always override unsafe receipt
        var safeReceipt = GenerateReceipt();

        // 🧪 Mock mode
        if (_isDevelopment)
        {
            var mockOrderId = $"order_mock_{Guid.NewGuid():N}";

            _logger.LogInformation(
                "🧪 MOCK Razorpay order | Id: {Id} | ₹{Amount} | Receipt: {Receipt}",
                mockOrderId, amount, safeReceipt);

            return new RazorpayOrderResult(
                mockOrderId, amount, currency, "created");
        }

        try
        {
            var client = new RazorpayClient(_keyId, _keySecret);

            var options = new Dictionary<string, object>
            {
                { "amount", (int)(amount * 100) },
                { "currency", currency },
                { "receipt", safeReceipt },
                { "payment_capture", 1 }
            };

            _logger.LogInformation(
                "📤 Creating Razorpay order | Receipt: {Receipt} | Length: {Length}",
                safeReceipt, safeReceipt.Length);

            var order = client.Order.Create(options);

            string id = order["id"].ToString()!;
            string stat = order["status"].ToString()!;

            _logger.LogInformation(
                "💳 Razorpay order created | Id: {Id} | ₹{Amount}",
                id, amount);

            return new RazorpayOrderResult(
                id, amount, currency, stat);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Razorpay order creation failed");
            throw;
        }
    }

    public bool VerifySignature(
        string razorpayOrderId,
        string razorpayPaymentId,
        string razorpaySignature)
    {
        if (_isDevelopment)
        {
            _logger.LogInformation("🧪 MOCK signature verification — VALID");
            return true;
        }

        try
        {
            var payload = $"{razorpayOrderId}|{razorpayPaymentId}";

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
                "🔐 Signature verification: {Result}",
                isValid ? "VALID ✅" : "INVALID ❌");

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Signature verification error");
            return false;
        }
    }

    public async Task<string> ProcessRefundAsync(
        string razorpayPaymentId,
        decimal amount,
        CancellationToken ct = default)
    {
        if (_isDevelopment)
        {
            var mockRefundId = $"rfnd_mock_{Guid.NewGuid():N}";

            _logger.LogInformation(
                "🧪 MOCK refund | Id: {Id} | ₹{Amount}",
                mockRefundId, amount);

            return mockRefundId;
        }

        try
        {
            var client = new RazorpayClient(_keyId, _keySecret);

            var payment = client.Payment.Fetch(razorpayPaymentId);

            var refund = payment.Refund(
                new Dictionary<string, object>
                {
                    { "amount", (int)(amount * 100) }
                });

            string refundId = refund["id"].ToString()!;

            _logger.LogInformation(
                "💰 Refund processed | Id: {Id} | ₹{Amount}",
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