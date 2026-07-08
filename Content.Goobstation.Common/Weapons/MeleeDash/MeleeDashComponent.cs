// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Common.Weapons.MeleeDash;

[RegisterComponent, NetworkedComponent]
public sealed partial class MeleeDashComponent : Component
{
    [DataField]
    public string? EmoteOnDash = "Flip"; // this sucks to have to turn into a fucking string but i dont have access to content prototypes

    [DataField]
    public SoundSpecifier? DashSound = new SoundPathSpecifier("/Audio/_Goobstation/Weapons/Effects/throwhard.ogg");

    [DataField("force")]
    public float DashForce = 15f;

    [DataField("length")]
    public float MaxDashLength = 4f;
}

[Serializable, NetSerializable]
public sealed class MeleeDashEvent(NetEntity weapon, Vector2 direction) : EntityEventArgs
{
    public readonly NetEntity Weapon = weapon;
    public readonly Vector2 Direction = direction;
}
