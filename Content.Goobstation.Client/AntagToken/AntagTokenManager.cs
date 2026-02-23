using Content.Goobstation.Common.AntagToken;
using Content.Goobstation.Common.ServerCurrency;
using Robust.Shared.Network;

namespace Content.Goobstation.Client.AntagToken;

public sealed class ClientAntagTokenManager : IAntagTokenManager, IPostInjectInit
{
    [Dependency] private readonly INetManager _net = default!;

    public int TokenCount { get; private set; }
    public bool OnCooldown { get; private set; }

    void IPostInjectInit.PostInject()
    {
        _net.RegisterNetMessage<MsgAntagTokenCountUpdate>(OnTokenCountUpdate);
        _net.RegisterNetMessage<MsgAntagTokenActivate>();
        _net.RegisterNetMessage<MsgAntagTokenDeactivate>();
        _net.RegisterNetMessage<MsgAntagTokenCountRequest>();
    }

    private void OnTokenCountUpdate(MsgAntagTokenCountUpdate msg)
    {
        TokenCount = msg.TokenCount;
        OnCooldown = msg.OnCooldown;
    }

    public void RequestTokenCount()
    {
        _net.ClientSendMessage(new MsgAntagTokenCountRequest());
    }

    public void SendActivate()
    {
        _net.ClientSendMessage(new MsgAntagTokenActivate());
    }

    public void SendDeactivate()
    {
        _net.ClientSendMessage(new MsgAntagTokenDeactivate());
    }

    // Server-side stubs
    bool IAntagTokenManager.HasActiveToken(NetUserId userId) => false;
    float IAntagTokenManager.GetWeightMultiplier() => 1f;
    void IAntagTokenManager.ConsumeToken(NetUserId userId, int roundId) { }
    void IAntagTokenManager.DeactivateToken(NetUserId userId) { }
    void IAntagTokenManager.ClearActiveTokens() { }
    IReadOnlyCollection<NetUserId> IAntagTokenManager.GetActiveTokenUsers() => Array.Empty<NetUserId>();
    void IAntagTokenManager.RefreshTokenCount(NetUserId userId) { }
}
