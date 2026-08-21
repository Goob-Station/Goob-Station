using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Content.Goobstation.Server.Twitch.Bits;

public sealed class TwitchChatOAuthManager : IAsyncDisposable
{
    private static readonly Uri ValidateUri = new("https://id.twitch.tv/oauth2/validate");
    private static readonly Uri TokenUri = new("https://id.twitch.tv/oauth2/token");

    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly Action<string, string> _onTokenChanged;
    private readonly Action<string> _onWarning;
    private readonly HttpClient _http = new();
    private readonly CancellationTokenSource _cancellation = new();
    private string _accessToken;
    private string _refreshToken;
    private Task? _runTask;

    public TwitchChatOAuthManager(
        string accessToken,
        string refreshToken,
        string clientId,
        string clientSecret,
        Action<string, string> onTokenChanged,
        Action<string> onWarning)
    {
        _accessToken = NormalizeAccessToken(accessToken);
        _refreshToken = refreshToken.Trim();
        _clientId = clientId.Trim();
        _clientSecret = clientSecret.Trim();
        _onTokenChanged = onTokenChanged;
        _onWarning = onWarning;
    }

    public void Start()
    {
        _runTask ??= Task.Run(RunAsync);
    }

    private async Task RunAsync()
    {
        var delay = TimeSpan.Zero;
        while (!_cancellation.IsCancellationRequested)
        {
            try
            {
                if (delay > TimeSpan.Zero)
                    await Task.Delay(delay, _cancellation.Token);

                var validation = await ValidateAsync(_cancellation.Token);
                if (validation.Valid && validation.ExpiresIn > 3600)
                {
                    delay = TimeSpan.FromSeconds(Math.Clamp(validation.ExpiresIn - 1800, 300, 3600));
                    continue;
                }

                if (!CanRefresh())
                {
                    _onWarning("The Twitch chat token is expired or near expiry, but automatic refresh credentials are not configured.");
                    delay = TimeSpan.FromMinutes(5);
                    continue;
                }

                var refreshed = await RefreshAsync(_cancellation.Token);
                _accessToken = refreshed.AccessToken;
                if (!string.IsNullOrWhiteSpace(refreshed.RefreshToken))
                    _refreshToken = refreshed.RefreshToken;
                _onTokenChanged(_accessToken, _refreshToken);
                delay = TimeSpan.FromSeconds(Math.Clamp(refreshed.ExpiresIn - 1800, 300, 3600));
            }
            catch (OperationCanceledException) when (_cancellation.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _onWarning($"Twitch chat OAuth check failed: {exception.Message}");
                delay = TimeSpan.FromMinutes(5);
            }
        }
    }

    private async Task<TokenValidation> ValidateAsync(CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, ValidateUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", _accessToken);
        using var response = await _http.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            return new TokenValidation(false, 0);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<ValidationResponse>(stream, cancellationToken: cancellationToken);
        if (payload == null || payload.ClientId != _clientId && !string.IsNullOrEmpty(_clientId))
            return new TokenValidation(false, 0);
        return new TokenValidation(true, payload.ExpiresIn);
    }

    private async Task<RefreshResponse> RefreshAsync(CancellationToken cancellationToken)
    {
        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = _clientId,
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = _refreshToken,
        };
        if (!string.IsNullOrEmpty(_clientSecret))
            parameters["client_secret"] = _clientSecret;
        using var content = new FormUrlEncodedContent(parameters);
        using var response = await _http.PostAsync(TokenUri, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<RefreshResponse>(stream, cancellationToken: cancellationToken);
        if (payload == null || string.IsNullOrWhiteSpace(payload.AccessToken))
            throw new InvalidOperationException("Twitch returned an invalid token refresh response.");
        return payload;
    }

    private bool CanRefresh()
    {
        return !string.IsNullOrEmpty(_refreshToken) &&
               !string.IsNullOrEmpty(_clientId);
    }

    private static string NormalizeAccessToken(string accessToken)
    {
        accessToken = accessToken.Trim();
        return accessToken.StartsWith("oauth:", StringComparison.OrdinalIgnoreCase)
            ? accessToken[6..]
            : accessToken;
    }

    public async ValueTask DisposeAsync()
    {
        _cancellation.Cancel();
        if (_runTask != null)
        {
            try
            {
                await _runTask;
            }
            catch (OperationCanceledException)
            {
            }
        }
        _http.Dispose();
        _cancellation.Dispose();
    }

    private sealed record TokenValidation(bool Valid, int ExpiresIn);

    private sealed record ValidationResponse(
        [property: JsonPropertyName("client_id")] string ClientId,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);

    public sealed record RefreshResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("refresh_token")] string? RefreshToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);
}
