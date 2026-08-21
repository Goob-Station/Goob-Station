using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Content.Goobstation.Server.Twitch;

namespace Content.IntegrationTests.Tests.Goobstation.Twitch;

[TestFixture]
public sealed class TwitchExtensionJwtValidatorTests
{
    private const string ChannelId = "40710601";
    private static readonly DateTimeOffset Now = DateTimeOffset.FromUnixTimeSeconds(2_000_000_000);
    private static readonly byte[] Secret = Encoding.UTF8.GetBytes("a-test-secret-that-is-at-least-32-bytes-long");

    [Test]
    public void AcceptsValidViewerToken()
    {
        var token = CreateToken(new
        {
            channel_id = ChannelId,
            exp = Now.ToUnixTimeSeconds() + 60,
            opaque_user_id = "U-viewer",
            user_id = "1234",
            role = "viewer",
            is_unlinked = false,
        });

        var valid = TwitchExtensionJwtValidator.TryValidate(
            token,
            Secret,
            ChannelId,
            Now,
            out var identity,
            out var error);

        Assert.That(valid, Is.True);
        Assert.That(error, Is.EqualTo(TwitchExtensionJwtValidationError.None));
        Assert.That(identity, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(identity!.ChannelId, Is.EqualTo(ChannelId));
            Assert.That(identity.OpaqueUserId, Is.EqualTo("U-viewer"));
            Assert.That(identity.UserId, Is.EqualTo("1234"));
            Assert.That(identity.Role, Is.EqualTo(TwitchExtensionRole.Viewer));
            Assert.That(identity.IsLinked, Is.True);
        });
    }

    [TestCase("moderator", TwitchExtensionRole.Moderator)]
    [TestCase("editor", TwitchExtensionRole.Editor)]
    [TestCase("broadcaster", TwitchExtensionRole.Broadcaster)]
    public void AcceptsModerationRoles(string role, TwitchExtensionRole expected)
    {
        var token = CreateToken(new
        {
            channel_id = ChannelId,
            exp = Now.ToUnixTimeSeconds() + 60,
            opaque_user_id = "U-staff",
            user_id = "1234",
            role,
            is_unlinked = false,
        });

        var valid = TwitchExtensionJwtValidator.TryValidate(
            token,
            Secret,
            ChannelId,
            Now,
            out var identity,
            out var error);

        Assert.Multiple(() =>
        {
            Assert.That(valid, Is.True);
            Assert.That(error, Is.EqualTo(TwitchExtensionJwtValidationError.None));
            Assert.That(identity?.Role, Is.EqualTo(expected));
        });
    }

    [Test]
    public void RejectsModifiedSignature()
    {
        var token = CreateToken(DefaultPayload());
        var segments = token.Split('.');
        segments[2] = (segments[2][0] == 'A' ? 'B' : 'A') + segments[2][1..];
        token = string.Join('.', segments);

        AssertRejected(token, TwitchExtensionJwtValidationError.InvalidSignature);
    }

    [Test]
    public void RejectsExpiredToken()
    {
        var token = CreateToken(new
        {
            channel_id = ChannelId,
            exp = Now.ToUnixTimeSeconds(),
            opaque_user_id = "U-viewer",
            role = "viewer",
        });

        AssertRejected(token, TwitchExtensionJwtValidationError.Expired);
    }

    [Test]
    public void AcceptsTokenForAnotherChannelWithoutRestriction()
    {
        var token = CreateToken(new
        {
            channel_id = "99999999",
            exp = Now.ToUnixTimeSeconds() + 60,
            opaque_user_id = "U-viewer",
            role = "viewer",
        });

        var valid = TwitchExtensionJwtValidator.TryValidate(
            token,
            Secret,
            null,
            Now,
            out var identity,
            out var error);

        Assert.Multiple(() =>
        {
            Assert.That(valid, Is.True);
            Assert.That(error, Is.EqualTo(TwitchExtensionJwtValidationError.None));
            Assert.That(identity?.ChannelId, Is.EqualTo("99999999"));
        });
    }

    [Test]
    public void RejectsExternalRole()
    {
        var token = CreateToken(new
        {
            channel_id = ChannelId,
            exp = Now.ToUnixTimeSeconds() + 60,
            opaque_user_id = "U-viewer",
            role = "external",
        });

        AssertRejected(token, TwitchExtensionJwtValidationError.InvalidRole);
    }

    [Test]
    public void RejectsUnsupportedAlgorithm()
    {
        var token = CreateToken(DefaultPayload(), "none");

        AssertRejected(token, TwitchExtensionJwtValidationError.UnsupportedAlgorithm);
    }

    private static object DefaultPayload()
    {
        return new
        {
            channel_id = ChannelId,
            exp = Now.ToUnixTimeSeconds() + 60,
            opaque_user_id = "U-viewer",
            role = "viewer",
        };
    }

    private static string CreateToken(object payload, string algorithm = "HS256")
    {
        var headerSegment = EncodeBase64Url(JsonSerializer.SerializeToUtf8Bytes(new
        {
            alg = algorithm,
            typ = "JWT",
        }));
        var payloadSegment = EncodeBase64Url(JsonSerializer.SerializeToUtf8Bytes(payload));
        var signingInput = $"{headerSegment}.{payloadSegment}";
        var signature = HMACSHA256.HashData(Secret, Encoding.ASCII.GetBytes(signingInput));
        return $"{signingInput}.{EncodeBase64Url(signature)}";
    }

    private static string EncodeBase64Url(byte[] value)
    {
        return Convert.ToBase64String(value)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static void AssertRejected(string token, TwitchExtensionJwtValidationError expectedError)
    {
        var valid = TwitchExtensionJwtValidator.TryValidate(
            token,
            Secret,
            ChannelId,
            Now,
            out var identity,
            out var error);

        Assert.Multiple(() =>
        {
            Assert.That(valid, Is.False);
            Assert.That(identity, Is.Null);
            Assert.That(error, Is.EqualTo(expectedError));
        });
    }
}
