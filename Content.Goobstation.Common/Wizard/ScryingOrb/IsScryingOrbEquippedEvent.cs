using Robust.Shared.Serialization;

namespace Content.Goobstation.Common.Wizard.ScryingOrb;

public struct IsScryingOrbEquippedEvent(NetEntity user)
{
    public readonly NetEntity User = user;
    public bool Equipped;
}
