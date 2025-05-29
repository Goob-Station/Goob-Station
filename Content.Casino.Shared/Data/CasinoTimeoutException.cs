using System;

namespace Content.Casino.Shared.Data;

/// <summary>
/// Exception thrown when a casino operation times out.
/// </summary>
public sealed class CasinoTimeoutException : Exception
{
    public CasinoTimeoutException() : base("Casino operation timed out") { }

    public CasinoTimeoutException(string message) : base(message) { }

    public CasinoTimeoutException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when a casino game operation fails.
/// </summary>
public sealed class CasinoGameException : Exception
{
    public string? GameId { get; }
    public string? SessionId { get; }

    public CasinoGameException() : base("Casino game operation failed") { }

    public CasinoGameException(string message) : base(message) { }

    public CasinoGameException(string message, Exception innerException) : base(message, innerException) { }

    public CasinoGameException(string message, string? gameId, string? sessionId) : base(message)
    {
        GameId = gameId;
        SessionId = sessionId;
    }
}
