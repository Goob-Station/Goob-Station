using Robust.Shared.Network;

namespace Content.Goobstation.Common.AntagToken;

public interface IAntagTokenManager
{
    void Initialize();

    bool HasActiveToken(NetUserId userId);
    float GetWeightMultiplier();
    void ConsumeToken(NetUserId userId, int roundId);
    void DeactivateToken(NetUserId userId);
    void ClearActiveTokens();
    IReadOnlyCollection<NetUserId> GetActiveTokenUsers();
    void RefreshTokenCount(NetUserId userId);

    int TokenCount { get; }
    bool OnCooldown { get; }
    void RequestTokenCount();
    void SendActivate();
    void SendDeactivate();
}
