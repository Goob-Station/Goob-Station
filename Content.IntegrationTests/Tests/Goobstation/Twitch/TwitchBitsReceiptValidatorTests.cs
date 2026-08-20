using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Content.Goobstation.Server.Twitch;

namespace Content.IntegrationTests.Tests.Goobstation.Twitch;

[TestFixture]
public sealed class TwitchBitsReceiptValidatorTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.FromUnixTimeSeconds(2_000_000_000);
    private static readonly byte[] Secret = Encoding.UTF8.GetBytes("a-test-secret-that-is-at-least-32-bytes-long");

    [Test]
    public void AcceptsValidReceipt()
    {
        var receipt = CreateReceipt(new
        {
            topic = "bits_transaction_receipt",
            exp = Now.ToUnixTimeSeconds() + 60,
            data = new
            {
                transactionId = "transaction-1",
                userId = "1234",
                product = new
                {
                    sku = "ss14-drop-held",
                },
            },
        });

        var valid = TwitchBitsReceiptValidator.TryValidate(
            receipt,
            Secret,
            Now,
            out var transaction,
            out var error);

        Assert.That(valid, Is.True);
        Assert.That(error, Is.EqualTo(TwitchBitsReceiptValidationError.None));
        Assert.That(transaction, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(transaction!.TransactionId, Is.EqualTo("transaction-1"));
            Assert.That(transaction.UserId, Is.EqualTo("1234"));
            Assert.That(transaction.Sku, Is.EqualTo("ss14-drop-held"));
            Assert.That(transaction.ExpiresAt, Is.EqualTo(Now.AddSeconds(60)));
        });
    }

    [Test]
    public void AcceptsStringExpirationAndProductSku()
    {
        var receipt = CreateReceipt(new
        {
            topic = "bits_transaction_receipt",
            exp = (Now.ToUnixTimeSeconds() + 60).ToString(),
            data = new
            {
                transactionId = "transaction-2",
                userId = "5678",
                productSku = "ss14-arm-nuke",
            },
        });

        var valid = TwitchBitsReceiptValidator.TryValidate(
            receipt,
            Secret,
            Now,
            out var transaction,
            out var error);

        Assert.That(valid, Is.True);
        Assert.That(error, Is.EqualTo(TwitchBitsReceiptValidationError.None));
        Assert.That(transaction?.Sku, Is.EqualTo("ss14-arm-nuke"));
    }

    [Test]
    public void RejectsModifiedSignature()
    {
        var receipt = CreateReceipt(DefaultPayload());
        var segments = receipt.Split('.');
        segments[2] = (segments[2][0] == 'A' ? 'B' : 'A') + segments[2][1..];

        AssertRejected(string.Join('.', segments), TwitchBitsReceiptValidationError.InvalidSignature);
    }

    [Test]
    public void RejectsWrongTopic()
    {
        var receipt = CreateReceipt(new
        {
            topic = "extension_transaction",
            exp = Now.ToUnixTimeSeconds() + 60,
            data = DefaultTransaction(),
        });

        AssertRejected(receipt, TwitchBitsReceiptValidationError.InvalidTopic);
    }

    [Test]
    public void RejectsExpiredReceipt()
    {
        var receipt = CreateReceipt(new
        {
            topic = "bits_transaction_receipt",
            exp = Now.ToUnixTimeSeconds(),
            data = DefaultTransaction(),
        });

        AssertRejected(receipt, TwitchBitsReceiptValidationError.Expired);
    }

    [Test]
    public void RejectsReceiptWithoutSku()
    {
        var receipt = CreateReceipt(new
        {
            topic = "bits_transaction_receipt",
            exp = Now.ToUnixTimeSeconds() + 60,
            data = new
            {
                transactionId = "transaction-1",
                userId = "1234",
            },
        });

        AssertRejected(receipt, TwitchBitsReceiptValidationError.MissingTransaction);
    }

    private static object DefaultPayload()
    {
        return new
        {
            topic = "bits_transaction_receipt",
            exp = Now.ToUnixTimeSeconds() + 60,
            data = DefaultTransaction(),
        };
    }

    private static object DefaultTransaction()
    {
        return new
        {
            transactionId = "transaction-1",
            userId = "1234",
            product = new
            {
                sku = "ss14-drop-held",
            },
        };
    }

    private static string CreateReceipt(object payload, string algorithm = "HS256")
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

    private static void AssertRejected(string receipt, TwitchBitsReceiptValidationError expectedError)
    {
        var valid = TwitchBitsReceiptValidator.TryValidate(
            receipt,
            Secret,
            Now,
            out var transaction,
            out var error);

        Assert.Multiple(() =>
        {
            Assert.That(valid, Is.False);
            Assert.That(transaction, Is.Null);
            Assert.That(error, Is.EqualTo(expectedError));
        });
    }
}
