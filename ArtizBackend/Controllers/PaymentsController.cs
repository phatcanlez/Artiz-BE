using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtizBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public PaymentsController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public record SePayCheckoutRequest(
        string OrderInvoiceNumber,
        long OrderAmount,
        string Currency,
        string OrderDescription,
        string? PaymentMethod,
        string? CustomerId);

    public record SePayCheckoutResponse(
        string CheckoutUrl,
        IDictionary<string, string> Fields);

    /// <summary>
    /// Tạo thông tin checkout SePay (sandbox) để FE render form.
    /// </summary>
    [HttpPost("sepay/checkout")]
    [AllowAnonymous]
    public ActionResult<SePayCheckoutResponse> CreateSePayCheckout([FromBody] SePayCheckoutRequest request)
    {
        var sepaySection = _configuration.GetSection("SePay");
        var environment = sepaySection["Environment"] ?? "sandbox";
        var merchantId = sepaySection["MerchantId"] ?? throw new InvalidOperationException("SePay:MerchantId is not configured");
        var secretKey = sepaySection["SecretKey"] ?? throw new InvalidOperationException("SePay:SecretKey is not configured");
        var sandboxUrl = sepaySection["SandboxCheckoutUrl"] ?? "https://pay-sandbox.sepay.vn/v1/checkout/init";
        var productionUrl = sepaySection["ProductionCheckoutUrl"] ?? "https://pay.sepay.vn/v1/checkout/init";

        var successTemplate = sepaySection["SuccessUrl"] ?? "https://example.com/order/{orderId}?payment=success";
        var errorTemplate = sepaySection["ErrorUrl"] ?? "https://example.com/order/{orderId}?payment=error";
        var cancelTemplate = sepaySection["CancelUrl"] ?? "https://example.com/order/{orderId}?payment=cancel";

        var orderId = request.OrderInvoiceNumber;

        var successUrl = successTemplate.Replace("{orderId}", orderId);
        var errorUrl = errorTemplate.Replace("{orderId}", orderId);
        var cancelUrl = cancelTemplate.Replace("{orderId}", orderId);

        var operation = "PURCHASE";
        var currency = string.IsNullOrWhiteSpace(request.Currency) ? "VND" : request.Currency;
        var paymentMethod = string.IsNullOrWhiteSpace(request.PaymentMethod) ? null : request.PaymentMethod;

        var fields = new Dictionary<string, string?>
        {
            ["merchant"] = merchantId,
            ["operation"] = operation,
            ["payment_method"] = paymentMethod,
            ["order_amount"] = request.OrderAmount.ToString(),
            ["currency"] = currency,
            ["order_invoice_number"] = request.OrderInvoiceNumber,
            ["order_description"] = request.OrderDescription,
            ["customer_id"] = string.IsNullOrWhiteSpace(request.CustomerId) ? null : request.CustomerId,
            ["success_url"] = successUrl,
            ["error_url"] = errorUrl,
            ["cancel_url"] = cancelUrl
        };

        var signature = SignFields(fields, secretKey);

        var outputFields = new Dictionary<string, string>(fields.Count + 1);
        foreach (var kv in fields)
        {
            if (kv.Value != null)
                outputFields[kv.Key] = kv.Value;
        }

        outputFields["signature"] = signature;

        var checkoutUrl = environment.Equals("production", StringComparison.OrdinalIgnoreCase)
            ? productionUrl
            : sandboxUrl;

        return Ok(new SePayCheckoutResponse(
            CheckoutUrl: checkoutUrl,
            Fields: outputFields));
    }

    private static string SignFields(IDictionary<string, string?> fields, string secretKey)
    {
        var signedFieldsOrder = new[]
        {
            "merchant",
            "operation",
            "payment_method",
            "order_amount",
            "currency",
            "order_invoice_number",
            "order_description",
            "customer_id",
            "success_url",
            "error_url",
            "cancel_url"
        };

        var parts = new List<string>();
        foreach (var field in signedFieldsOrder)
        {
            if (!fields.TryGetValue(field, out var value) || value == null)
                continue;

            parts.Add($"{field}={value}");
        }

        var signedString = string.Join(",", parts);

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedString));
        return Convert.ToBase64String(hash);
    }
}

