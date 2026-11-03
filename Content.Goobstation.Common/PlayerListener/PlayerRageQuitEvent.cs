using Robust.Shared.Network;

namespace Content.Goobstation.Common.PlayerListener;

public sealed class PlayerRageQuitEvent(NetUserId userId) : EntityEventArgs
{
    public readonly NetUserId UserId = userId;
}
