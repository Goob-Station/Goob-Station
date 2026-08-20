using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Content.Goobstation.Common.CCVar;
using Robust.Server.ServerStatus;
using Robust.Shared.Asynchronous;
using Robust.Shared.Configuration;

namespace Content.Goobstation.Server.Twitch;

public sealed class TwitchApiManager : ITwitchApiManager
{
    private const int MinimumBodySize = 1024;
    private const int MaximumBodySize = 1024 * 1024;
    private const string SawmillName = "twitch.api";

    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly IStatusHost _statusHost = default!;
    [Dependency] private readonly ITaskManager _taskManager = default!;

    private readonly object _routeLock = new();
    private readonly Dictionary<RouteKey, Route> _routes = new();
    private readonly object _originLock = new();
    private readonly object _secretLock = new();
    private HashSet<string> _allowedOrigins = new(StringComparer.OrdinalIgnoreCase);
    private readonly System.Runtime.CompilerServices.ConditionalWeakTable<IStatusHandlerContext, TwitchExtensionIdentity>
        _extensionIdentities = new();

    private ISawmill _sawmill = default!;
    private volatile bool _enabled;
    private volatile string _extensionChannelId = string.Empty;
    private volatile string _extensionClientId = string.Empty;
    private byte[] _extensionSecret = [];
    private volatile int _maxRequestBodySize = 64 * 1024;
    private volatile string _serverToken = string.Empty;
    private bool _initialized;

    public bool Enabled => _enabled;

    public void Initialize()
    {
        if (_initialized)
            return;

        _initialized = true;
        _sawmill = _logManager.GetSawmill(SawmillName);

        RegisterRouteInternal(
            HttpMethod.Get,
            "/health",
            HandleHealth,
            TwitchApiAccess.Public,
            availableWhenDisabled: true);
        RegisterRouteInternal(
            HttpMethod.Get,
            "/extension/session",
            HandleExtensionSession,
            TwitchApiAccess.ExtensionJwt,
            availableWhenDisabled: false);

        _configuration.OnValueChanged(GoobCVars.TwitchExtensionClientId, OnExtensionClientIdChanged, true);
        _configuration.OnValueChanged(GoobCVars.TwitchExtensionChannelId, OnExtensionChannelIdChanged, true);
        _configuration.OnValueChanged(GoobCVars.TwitchExtensionSecret, OnExtensionSecretChanged, true);
        _configuration.OnValueChanged(GoobCVars.TwitchApiEnabled, OnEnabledChanged, true);
        _configuration.OnValueChanged(GoobCVars.TwitchApiToken, OnTokenChanged, true);
        _configuration.OnValueChanged(GoobCVars.TwitchApiAllowedOrigins, OnAllowedOriginsChanged, true);
        _configuration.OnValueChanged(GoobCVars.TwitchApiMaxRequestBodySize, OnMaxRequestBodySizeChanged, true);

        _statusHost.AddHandler(HandleRequest);
        _sawmill.Info($"Twitch API routes registered under {ITwitchApiManager.ApiPrefix}");
    }

    public void Shutdown()
    {
        if (!_initialized)
            return;

        _configuration.UnsubValueChanged(GoobCVars.TwitchExtensionClientId, OnExtensionClientIdChanged);
        _configuration.UnsubValueChanged(GoobCVars.TwitchExtensionChannelId, OnExtensionChannelIdChanged);
        _configuration.UnsubValueChanged(GoobCVars.TwitchExtensionSecret, OnExtensionSecretChanged);
        _configuration.UnsubValueChanged(GoobCVars.TwitchApiEnabled, OnEnabledChanged);
        _configuration.UnsubValueChanged(GoobCVars.TwitchApiToken, OnTokenChanged);
        _configuration.UnsubValueChanged(GoobCVars.TwitchApiAllowedOrigins, OnAllowedOriginsChanged);
        _configuration.UnsubValueChanged(GoobCVars.TwitchApiMaxRequestBodySize, OnMaxRequestBodySizeChanged);

        lock (_secretLock)
        {
            CryptographicOperations.ZeroMemory(_extensionSecret);
            _extensionSecret = [];
        }
    }

    public void RegisterRoute(
        HttpMethod method,
        string path,
        TwitchApiRouteHandler handler,
        TwitchApiAccess access = TwitchApiAccess.ServerToken)
    {
        RegisterRouteInternal(method, path, handler, access, availableWhenDisabled: false);
    }

    public bool TryGetExtensionIdentity(
        IStatusHandlerContext context,
        [NotNullWhen(true)] out TwitchExtensionIdentity? identity)
    {
        return _extensionIdentities.TryGetValue(context, out identity);
    }

    public async Task<T?> ReadJsonAsync<T>(
        IStatusHandlerContext context,
        JsonSerializerOptions? options = null)
    {
        await using var body = new MemoryStream();
        var buffer = new byte[8192];
        var total = 0;

        while (true)
        {
            var read = await context.RequestBody.ReadAsync(buffer.AsMemory());
            if (read == 0)
                break;

            total += read;
            if (total > _maxRequestBodySize)
                throw new TwitchApiRequestTooLargeException();

            await body.WriteAsync(buffer.AsMemory(0, read));
        }

        body.Position = 0;
        return await JsonSerializer.DeserializeAsync<T>(body, options);
    }

    public async Task RunOnMainThread(Action action)
    {
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _taskManager.RunOnMainThread(() =>
        {
            try
            {
                action();
                completion.TrySetResult();
            }
            catch (Exception exception)
            {
                completion.TrySetException(exception);
            }
        });

        await completion.Task;
    }

    public async Task<T> RunOnMainThread<T>(Func<T> action)
    {
        var completion = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        _taskManager.RunOnMainThread(() =>
        {
            try
            {
                completion.TrySetResult(action());
            }
            catch (Exception exception)
            {
                completion.TrySetException(exception);
            }
        });

        return await completion.Task;
    }

    private void RegisterRouteInternal(
        HttpMethod method,
        string path,
        TwitchApiRouteHandler handler,
        TwitchApiAccess access,
        bool availableWhenDisabled)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(handler);

        var fullPath = ITwitchApiManager.ApiPrefix + NormalizePath(path);
        var key = new RouteKey(method.Method.ToUpperInvariant(), fullPath);

        lock (_routeLock)
        {
            if (!_routes.TryAdd(key, new Route(handler, access, availableWhenDisabled)))
                throw new InvalidOperationException($"A Twitch API route is already registered for {method} {fullPath}");
        }
    }

    private async Task<bool> HandleRequest(IStatusHandlerContext context)
    {
        var path = context.Url.AbsolutePath;
        if (path != ITwitchApiManager.ApiPrefix &&
            !path.StartsWith(ITwitchApiManager.ApiPrefix + "/", StringComparison.Ordinal))
        {
            return false;
        }

        var allowedMethods = GetAllowedMethods(path);
        if (context.RequestMethod == HttpMethod.Options)
        {
            await HandlePreflight(context, allowedMethods);
            return true;
        }

        ApplyCors(context, out var originAllowed);
        if (!originAllowed)
        {
            await RespondError(context, HttpStatusCode.Forbidden, "origin_not_allowed", "The request origin is not allowed.");
            return true;
        }

        Route? route;
        lock (_routeLock)
        {
            _routes.TryGetValue(
                new RouteKey(context.RequestMethod.Method.ToUpperInvariant(), path),
                out route);
        }

        if (route == null)
        {
            if (allowedMethods.Count != 0)
            {
                context.ResponseHeaders["Allow"] = string.Join(", ", allowedMethods);
                await RespondError(
                    context,
                    HttpStatusCode.MethodNotAllowed,
                    "method_not_allowed",
                    "The HTTP method is not supported for this route.");
            }
            else
            {
                await RespondError(context, HttpStatusCode.NotFound, "route_not_found", "The Twitch API route was not found.");
            }

            return true;
        }

        if (!_enabled && !route.AvailableWhenDisabled)
        {
            await RespondError(
                context,
                HttpStatusCode.ServiceUnavailable,
                "integration_disabled",
                "The Twitch integration is disabled.");
            return true;
        }

        if (!RequestBodyWithinAdvertisedLimit(context))
        {
            await RespondError(
                context,
                HttpStatusCode.RequestEntityTooLarge,
                "request_too_large",
                "The request body exceeds the configured limit.");
            return true;
        }

        TwitchExtensionIdentity? extensionIdentity = null;
        switch (route.Access)
        {
            case TwitchApiAccess.ServerToken when !await CheckServerToken(context):
                return true;
            case TwitchApiAccess.ExtensionJwt:
                extensionIdentity = await CheckExtensionJwt(context);
                if (extensionIdentity == null)
                    return true;
                break;
        }

        if (extensionIdentity != null)
        {
            _extensionIdentities.Remove(context);
            _extensionIdentities.Add(context, extensionIdentity);
        }

        try
        {
            await route.Handler(context);
        }
        catch (TwitchApiRequestTooLargeException)
        {
            await RespondError(
                context,
                HttpStatusCode.RequestEntityTooLarge,
                "request_too_large",
                "The request body exceeds the configured limit.");
        }
        catch (JsonException)
        {
            await RespondError(context, HttpStatusCode.BadRequest, "invalid_json", "The request body contains invalid JSON.");
        }
        catch (Exception exception)
        {
            _sawmill.Error($"Unhandled exception in {context.RequestMethod} {path}: {exception}");
            await RespondError(
                context,
                HttpStatusCode.InternalServerError,
                "internal_error",
                "The Twitch API encountered an internal error.");
        }
        finally
        {
            _extensionIdentities.Remove(context);
        }

        return true;
    }

    private async Task HandlePreflight(IStatusHandlerContext context, IReadOnlyList<string> allowedMethods)
    {
        ApplyCors(context, out var originAllowed);
        if (!originAllowed)
        {
            await RespondError(context, HttpStatusCode.Forbidden, "origin_not_allowed", "The request origin is not allowed.");
            return;
        }

        if (allowedMethods.Count == 0)
        {
            await RespondError(context, HttpStatusCode.NotFound, "route_not_found", "The Twitch API route was not found.");
            return;
        }

        context.ResponseHeaders["Access-Control-Allow-Methods"] = string.Join(", ", allowedMethods);
        context.ResponseHeaders["Access-Control-Allow-Headers"] = "Authorization, Content-Type, X-Extension-JWT";
        context.ResponseHeaders["Access-Control-Max-Age"] = "600";
        await context.RespondNoContentAsync();
    }

    private async Task<bool> CheckServerToken(IStatusHandlerContext context)
    {
        var configuredToken = _serverToken;
        if (string.IsNullOrEmpty(configuredToken))
        {
            await RespondError(
                context,
                HttpStatusCode.ServiceUnavailable,
                "authentication_not_configured",
                "Server-token authentication is not configured.");
            return false;
        }

        if (!context.RequestHeaders.TryGetValue("Authorization", out var authorization))
        {
            context.ResponseHeaders["WWW-Authenticate"] = "Bearer";
            await RespondError(
                context,
                HttpStatusCode.Unauthorized,
                "authentication_required",
                "A bearer token is required.");
            return false;
        }

        var header = authorization.ToString();
        const string bearerPrefix = "Bearer ";
        if (!header.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            context.ResponseHeaders["WWW-Authenticate"] = "Bearer";
            await RespondError(
                context,
                HttpStatusCode.Unauthorized,
                "authentication_invalid",
                "The authorization scheme is invalid.");
            return false;
        }

        var suppliedToken = header[bearerPrefix.Length..].Trim();
        if (CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(suppliedToken),
                Encoding.UTF8.GetBytes(configuredToken)))
        {
            return true;
        }

        context.ResponseHeaders["WWW-Authenticate"] = "Bearer";
        _sawmill.Warning($"Rejected unauthorized Twitch API request from {context.RemoteEndPoint}");
        await RespondError(
            context,
            HttpStatusCode.Unauthorized,
            "authentication_invalid",
            "The bearer token is invalid.");
        return false;
    }

    private async Task<TwitchExtensionIdentity?> CheckExtensionJwt(IStatusHandlerContext context)
    {
        if (!TryGetExtensionToken(context, out var token))
        {
            context.ResponseHeaders["WWW-Authenticate"] = "Bearer";
            await RespondError(
                context,
                HttpStatusCode.Unauthorized,
                "extension_authentication_required",
                "A Twitch Extension JWT is required.");
            return null;
        }

        TwitchExtensionIdentity? identity;
        TwitchExtensionJwtValidationError error;
        bool valid;
        lock (_secretLock)
        {
            valid = TwitchExtensionJwtValidator.TryValidate(
                token,
                _extensionSecret,
                _extensionChannelId,
                DateTimeOffset.UtcNow,
                out identity,
                out error);
        }

        if (valid)
        {
            return identity;
        }

        if (error == TwitchExtensionJwtValidationError.MissingConfiguration)
        {
            await RespondError(
                context,
                HttpStatusCode.ServiceUnavailable,
                "extension_authentication_not_configured",
                "Twitch Extension authentication is not configured.");
            return null;
        }

        context.ResponseHeaders["WWW-Authenticate"] = "Bearer";
        _sawmill.Warning($"Rejected Twitch Extension JWT from {context.RemoteEndPoint}: {error}");
        await RespondError(
            context,
            HttpStatusCode.Unauthorized,
            "extension_authentication_invalid",
            "The Twitch Extension JWT is invalid or expired.");
        return null;
    }

    private static bool TryGetExtensionToken(IStatusHandlerContext context, [NotNullWhen(true)] out string? token)
    {
        token = null;
        if (context.RequestHeaders.TryGetValue("X-Extension-JWT", out var extensionJwt))
        {
            token = extensionJwt.ToString().Trim();
            return !string.IsNullOrEmpty(token);
        }

        if (!context.RequestHeaders.TryGetValue("Authorization", out var authorization))
            return false;

        var header = authorization.ToString();
        const string bearerPrefix = "Bearer ";
        if (!header.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
            return false;

        token = header[bearerPrefix.Length..].Trim();
        return !string.IsNullOrEmpty(token);
    }

    private IReadOnlyList<string> GetAllowedMethods(string path)
    {
        lock (_routeLock)
        {
            return _routes.Keys
                .Where(key => key.Path == path)
                .Select(key => key.Method)
                .Order()
                .ToArray();
        }
    }

    private void ApplyCors(IStatusHandlerContext context, out bool originAllowed)
    {
        if (!context.RequestHeaders.TryGetValue("Origin", out var originHeader))
        {
            originAllowed = true;
            return;
        }

        var origin = originHeader.ToString();
        lock (_originLock)
        {
            originAllowed = _allowedOrigins.Contains(origin);
        }

        if (!originAllowed)
            return;

        context.ResponseHeaders["Access-Control-Allow-Origin"] = origin;
        context.ResponseHeaders["Vary"] = "Origin";
    }

    private bool RequestBodyWithinAdvertisedLimit(IStatusHandlerContext context)
    {
        if (!context.RequestHeaders.TryGetValue("Content-Length", out var contentLength))
            return true;

        return long.TryParse(contentLength.ToString(), out var length) &&
               length >= 0 &&
               length <= _maxRequestBodySize;
    }

    private static string NormalizePath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        path = path.Trim();
        if (!path.StartsWith('/'))
            path = "/" + path;

        if (path.Length > 1)
            path = path.TrimEnd('/');

        if (path.Contains('?') || path.Contains('#') || path.Contains("//", StringComparison.Ordinal))
            throw new ArgumentException("Twitch API route paths cannot contain queries, fragments, or empty segments.", nameof(path));

        foreach (var segment in path.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            if (segment is "." or "..")
                throw new ArgumentException("Twitch API route paths cannot contain relative segments.", nameof(path));
        }

        return path;
    }

    private Task HandleHealth(IStatusHandlerContext context)
    {
        return context.RespondJsonAsync(new HealthResponse(Enabled));
    }

    private Task HandleExtensionSession(IStatusHandlerContext context)
    {
        if (!TryGetExtensionIdentity(context, out var identity))
            throw new InvalidOperationException("The Extension identity is unavailable inside an authenticated route.");

        return context.RespondJsonAsync(new ExtensionSessionResponse(
            _extensionClientId,
            identity.ChannelId,
            identity.OpaqueUserId,
            identity.UserId,
            identity.Role.ToString().ToLowerInvariant(),
            identity.IsLinked));
    }

    private static Task RespondError(
        IStatusHandlerContext context,
        HttpStatusCode statusCode,
        string error,
        string message)
    {
        return context.RespondJsonAsync(new ErrorResponse(error, message), statusCode);
    }

    private void OnEnabledChanged(bool enabled)
    {
        _enabled = enabled;
        _sawmill.Info(enabled ? "Twitch API enabled" : "Twitch API disabled");
    }

    private void OnExtensionClientIdChanged(string clientId)
    {
        _extensionClientId = clientId.Trim();
    }

    private void OnExtensionChannelIdChanged(string channelId)
    {
        _extensionChannelId = channelId.Trim();
    }

    private void OnExtensionSecretChanged(string encodedSecret)
    {
        byte[] decoded = [];
        if (!string.IsNullOrWhiteSpace(encodedSecret))
        {
            try
            {
                decoded = Convert.FromBase64String(encodedSecret.Trim());
                if (decoded.Length < 32)
                {
                    CryptographicOperations.ZeroMemory(decoded);
                    decoded = [];
                    _sawmill.Warning("The Twitch Extension secret is shorter than 256 bits and will not be used.");
                }
            }
            catch (FormatException)
            {
                _sawmill.Warning("The Twitch Extension secret is not valid base64 and will not be used.");
            }
        }

        lock (_secretLock)
        {
            CryptographicOperations.ZeroMemory(_extensionSecret);
            _extensionSecret = decoded;
        }
    }

    private void OnTokenChanged(string token)
    {
        _serverToken = token;
    }

    private void OnAllowedOriginsChanged(string origins)
    {
        var parsed = origins
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        lock (_originLock)
        {
            _allowedOrigins = parsed;
        }
    }

    private void OnMaxRequestBodySizeChanged(int size)
    {
        _maxRequestBodySize = Math.Clamp(size, MinimumBodySize, MaximumBodySize);
        if (_maxRequestBodySize != size)
        {
            _sawmill.Warning(
                $"Configured Twitch API request body limit {size} is outside the supported range; using {_maxRequestBodySize} bytes.");
        }
    }

    private readonly record struct RouteKey(string Method, string Path);

    private sealed record Route(
        TwitchApiRouteHandler Handler,
        TwitchApiAccess Access,
        bool AvailableWhenDisabled);

    private sealed record HealthResponse(
        [property: JsonPropertyName("service")] string Service,
        [property: JsonPropertyName("apiVersion")] int ApiVersion,
        [property: JsonPropertyName("enabled")] bool Enabled)
    {
        public HealthResponse(bool enabled) : this("goob-station-twitch-api", 1, enabled)
        {
        }
    }

    private sealed record ErrorResponse(
        [property: JsonPropertyName("error")] string Error,
        [property: JsonPropertyName("message")] string Message);

    private sealed record ExtensionSessionResponse(
        [property: JsonPropertyName("clientId")] string ClientId,
        [property: JsonPropertyName("channelId")] string ChannelId,
        [property: JsonPropertyName("opaqueUserId")] string OpaqueUserId,
        [property: JsonPropertyName("userId")] string? UserId,
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("linked")] bool Linked);

    private sealed class TwitchApiRequestTooLargeException : Exception;
}
