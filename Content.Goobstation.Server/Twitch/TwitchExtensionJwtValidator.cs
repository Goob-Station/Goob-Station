using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Content.Goobstation.Server.Twitch;

public enum TwitchExtensionJwtValidationError
{
    None,
    MissingConfiguration,
    Malformed,
    UnsupportedAlgorithm,
    InvalidSignature,
    Expired,
    InvalidChannel,
    InvalidRole,
    MissingIdentity,
}

public static class TwitchExtensionJwtValidator
{
    private const int MaximumTokenLength = 16 * 1024;

    public static bool TryValidate(
        string token,
        ReadOnlySpan<byte> secret,
        string expectedChannelId,
        DateTimeOffset now,
        [NotNullWhen(true)] out TwitchExtensionIdentity? identity,
        out TwitchExtensionJwtValidationError error)
    {
        identity = null;
        error = TwitchExtensionJwtValidationError.Malformed;

        if (secret.IsEmpty || string.IsNullOrWhiteSpace(expectedChannelId))
        {
            error = TwitchExtensionJwtValidationError.MissingConfiguration;
            return false;
        }

        if (string.IsNullOrWhiteSpace(token) || token.Length > MaximumTokenLength)
            return false;

        var segments = token.Split('.');
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
                error = TwitchExtensionJwtValidationError.UnsupportedAlgorithm;
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
            error = TwitchExtensionJwtValidationError.InvalidSignature;
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

            if (!TryReadString(root, "channel_id", out var channelId) || channelId != expectedChannelId)
            {
                error = TwitchExtensionJwtValidationError.InvalidChannel;
                return false;
            }

            if (!root.TryGetProperty("exp", out var expirationElement) ||
                expirationElement.ValueKind != JsonValueKind.Number ||
                !expirationElement.TryGetInt64(out var expiration))
            {
                return false;
            }

            if (expiration <= now.ToUnixTimeSeconds())
            {
                error = TwitchExtensionJwtValidationError.Expired;
                return false;
            }

            if (!TryReadString(root, "role", out var roleName) || !TryParseRole(roleName, out var role))
            {
                error = TwitchExtensionJwtValidationError.InvalidRole;
                return false;
            }

            if (!TryReadString(root, "opaque_user_id", out var opaqueUserId))
            {
                error = TwitchExtensionJwtValidationError.MissingIdentity;
                return false;
            }

            string? userId = null;
            if (root.TryGetProperty("user_id", out var userIdElement) &&
                userIdElement.ValueKind == JsonValueKind.String)
            {
                userId = userIdElement.GetString();
            }

            var isUnlinked = root.TryGetProperty("is_unlinked", out var isUnlinkedElement) &&
                             isUnlinkedElement.ValueKind == JsonValueKind.True;

            identity = new TwitchExtensionIdentity(
                channelId,
                opaqueUserId,
                userId,
                role,
                !isUnlinked && !string.IsNullOrEmpty(userId));
            error = TwitchExtensionJwtValidationError.None;
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool TryReadString(JsonElement root, string property, [NotNullWhen(true)] out string? value)
    {
        value = null;
        if (!root.TryGetProperty(property, out var element) || element.ValueKind != JsonValueKind.String)
            return false;

        value = element.GetString();
        return !string.IsNullOrWhiteSpace(value);
    }

    private static bool TryParseRole(string role, out TwitchExtensionRole parsed)
    {
        parsed = role switch
        {
            "viewer" => TwitchExtensionRole.Viewer,
            "moderator" => TwitchExtensionRole.Moderator,
            "editor" => TwitchExtensionRole.Editor,
            "broadcaster" => TwitchExtensionRole.Broadcaster,
            _ => default,
        };

        return role is "viewer" or "moderator" or "editor" or "broadcaster";
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
