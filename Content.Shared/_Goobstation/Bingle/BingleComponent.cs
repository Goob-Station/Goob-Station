using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Content.Shared.Damage;
using Robust.Shared.Network;

namespace Content.Shared._Goobstation.Bingle;

[RegisterComponent, NetworkedComponent]
public sealed partial class BingleComponent : Component
{
    [DataField]
    public bool Upgraded = false;
    [DataField]
    public DamageSpecifier UpgradeDamage = default!;
    [DataField]
    public bool Prime = false;

    public EntityUid? MyPit;
}
[Serializable, NetSerializable]
public sealed class BingleUpgradeEntityMessage(NetEntity bingle) : EntityEventArgs
{
    public NetEntity Bingle = bingle;
}

[Serializable, NetSerializable]
public enum BingleVisual : byte
{
    Upgraded,
    Combat
}
