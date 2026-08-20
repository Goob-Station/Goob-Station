using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Robust.Server.ServerStatus;

namespace Content.Goobstation.Server.Twitch;

public enum TwitchApiAccess
{
    Public,
    ServerToken,
    ExtensionJwt,
}

public enum TwitchExtensionRole
{
    Viewer,
    Moderator,
    Broadcaster,
}

public sealed record TwitchExtensionIdentity(
    string ChannelId,
    string OpaqueUserId,
    string? UserId,
    TwitchExtensionRole Role,
    bool IsLinked);

public sealed record TwitchBitsTransaction(
    string TransactionId,
    string UserId,
    string Sku,
    DateTimeOffset ExpiresAt);

public delegate Task TwitchApiRouteHandler(IStatusHandlerContext context);

public interface ITwitchApiManager
{
    public const string ApiPrefix = "/twitch/api/v1";

    bool Enabled { get; }

    void Initialize();

    void Shutdown();

    void RegisterRoute(
        HttpMethod method,
        string path,
        TwitchApiRouteHandler handler,
        TwitchApiAccess access = TwitchApiAccess.ServerToken);

    bool TryGetExtensionIdentity(
        IStatusHandlerContext context,
        [NotNullWhen(true)] out TwitchExtensionIdentity? identity);

    bool TryValidateBitsReceipt(
        string receipt,
        DateTimeOffset now,
        [NotNullWhen(true)] out TwitchBitsTransaction? transaction,
        out TwitchBitsReceiptValidationError error);

    Task<T?> ReadJsonAsync<T>(IStatusHandlerContext context, JsonSerializerOptions? options = null);

    Task RunOnMainThread(Action action);

    Task<T> RunOnMainThread<T>(Func<T> action);
}
