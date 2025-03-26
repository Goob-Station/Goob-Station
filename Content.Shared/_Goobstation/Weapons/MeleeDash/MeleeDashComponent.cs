using System.Numerics;
using Content.Shared.Chat.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Weapons.MeleeDash;

[RegisterComponent, NetworkedComponent]
public sealed partial class MeleeDashComponent : Component
{
    [DataField]
    public ProtoId<EmotePrototype>? EmoteOnDash = "Flip";

    [DataField]
    public SoundSpecifier? DashSound = new SoundPathSpecifier("/Audio/_Goobstation/Weapons/Effects/throwhard.ogg");

    [DataField]
    public float DashForce = 15f;

    [DataField]
    public float MaxDashLength = 4f;
}

[Serializable, NetSerializable]
public sealed class MeleeDashEvent(NetEntity weapon, Vector2 direction) : EntityEventArgs
{
    public readonly NetEntity Weapon = weapon;
    public readonly Vector2 Direction = direction;
}
