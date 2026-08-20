using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Content.Goobstation.Server.Twitch;

public enum TwitchBitsReceiptValidationError
{
    None,
    MissingConfiguration,
    Malformed,
    UnsupportedAlgorithm,
    InvalidSignature,
    InvalidTopic,
    Expired,
    MissingTransaction,
}

public static class TwitchBitsReceiptValidator
{
    private const int MaximumTokenLength = 32 * 1024;

    public static bool TryValidate(
        string receipt,
        ReadOnlySpan<byte> secret,
        DateTimeOffset now,
        [NotNullWhen(true)] out TwitchBitsTransaction? transaction,
        out TwitchBitsReceiptValidationError error)
    {
        transaction = null;
        error = TwitchBitsReceiptValidationError.Malformed;

        if (secret.IsEmpty)
        {
            error = TwitchBitsReceiptValidationError.MissingConfiguration;
            return false;
        }

        if (string.IsNullOrWhiteSpace(receipt) || receipt.Length > MaximumTokenLength)
            return false;

        var segments = receipt.Split('.');
        if (segments.Length != 3 || segments.Any(string.IsNullOrEmpty))
            return false;

        byte[] headerBytes;
        byte[] suppliedSignature;
        try
        {
            headerBytes = DecodeBase64Url(segments[0]);
            suppliedSignature = DecodeBase64Url(segments[2]);
        }
        catch (FormatException)
        {
            return false;
        }

        try
        {
            using var header = JsonDocument.Parse(headerBytes);
            if (!header.RootElement.TryGetProperty("alg", out var algorithm) ||
                algorithm.ValueKind != JsonValueKind.String ||
                algorithm.GetString() != "HS256")
            {
                error = TwitchBitsReceiptValidationError.UnsupportedAlgorithm;
                return false;
            }
        }
        catch (JsonException)
        {
            return false;
        }

        var signedData = Encoding.ASCII.GetBytes($"{segments[0]}.{segments[1]}");
        var expectedSignature = HMACSHA256.HashData(secret, signedData);
        if (!CryptographicOperations.FixedTimeEquals(suppliedSignature, expectedSignature))
        {
            error = TwitchBitsReceiptValidationError.InvalidSignature;
            return false;
        }

        byte[] payloadBytes;
        try
        {
            payloadBytes = DecodeBase64Url(segments[1]);
        }
        catch (FormatException)
        {
            return false;
        }

        try
        {
            using var payload = JsonDocument.Parse(payloadBytes);
            var root = payload.RootElement;

            if (!TryReadString(root, "topic", out var topic) || topic != "bits_transaction_receipt")
            {
                error = TwitchBitsReceiptValidationError.InvalidTopic;
                return false;
            }

            if (!TryReadUnixTime(root, "exp", out var expiration))
                return false;

            if (expiration <= now.ToUnixTimeSeconds())
            {
                error = TwitchBitsReceiptValidationError.Expired;
                return false;
            }

            if (!root.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object ||
                !TryReadString(data, "transactionId", out var transactionId) ||
                !TryReadString(data, "userId", out var userId) ||
                !TryReadSku(data, out var sku))
            {
                error = TwitchBitsReceiptValidationError.MissingTransaction;
                return false;
            }

            if (transactionId.Length > 255 || userId.Length > 128 || sku.Length > 255)
            {
                error = TwitchBitsReceiptValidationError.MissingTransaction;
                return false;
            }

            transaction = new TwitchBitsTransaction(
                transactionId,
                userId,
                sku,
                DateTimeOffset.FromUnixTimeSeconds(expiration));
            error = TwitchBitsReceiptValidationError.None;
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool TryReadSku(JsonElement data, [NotNullWhen(true)] out string? sku)
    {
        sku = null;
        if (data.TryGetProperty("product", out var product))
        {
            if (product.ValueKind == JsonValueKind.Object && TryReadString(product, "sku", out sku))
                return true;

            if (product.ValueKind == JsonValueKind.String)
            {
                var value = product.GetString();
                if (!string.IsNullOrWhiteSpace(value) && value != "inDevelopment")
                {
                    sku = value;
                    return true;
                }
            }
        }

        return TryReadString(data, "productSku", out sku) || TryReadString(data, "sku", out sku);
    }

    private static bool TryReadUnixTime(JsonElement root, string property, out long value)
    {
        value = 0;
        if (!root.TryGetProperty(property, out var element))
            return false;

        if (element.ValueKind == JsonValueKind.Number)
            return element.TryGetInt64(out value);

        return element.ValueKind == JsonValueKind.String && long.TryParse(element.GetString(), out value);
    }

    private static bool TryReadString(JsonElement root, string property, [NotNullWhen(true)] out string? value)
    {
        value = null;
        if (!root.TryGetProperty(property, out var element) || element.ValueKind != JsonValueKind.String)
            return false;

        value = element.GetString();
        return !string.IsNullOrWhiteSpace(value);
    }

    private static byte[] DecodeBase64Url(string value)
    {
        if (value.Length % 4 == 1)
            throw new FormatException("Invalid base64url length.");

        var base64 = value.Replace('-', '+').Replace('_', '/');
        base64 += (value.Length % 4) switch
        {
            2 => "==",
            3 => "=",
            _ => string.Empty,
        };

        return Convert.FromBase64String(base64);
    }
}
