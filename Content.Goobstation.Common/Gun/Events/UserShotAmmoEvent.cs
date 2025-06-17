using Robust.Shared.Serialization;

namespace Content.Goobstation.Common.Gun.Events;

/// <summary>
/// Raised when entity shoots a gun.
/// </summary>
[Serializable, NetSerializable]
public sealed class UserShotAmmoEvent : EntityEventArgs
{
    public List<NetEntity> FiredProjectiles { get; }
    public NetEntity Gun { get; }

    public UserShotAmmoEvent(List<NetEntity> firedProjectiles, NetEntity gun)
    {
        FiredProjectiles = firedProjectiles;
        Gun = gun;
    }
}
