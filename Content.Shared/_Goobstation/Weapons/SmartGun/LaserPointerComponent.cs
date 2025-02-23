using System.Numerics;
using Content.Shared.Physics;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Goobstation.Weapons.SmartGun;

/// <summary>
/// Activates a laser pointer when wielding an item
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class LaserPointerComponent : Component
{
    [DataField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/_Goobstation/Weapons/Effects/laserpointer.ogg");

    [DataField(customTypeSerializer: typeof(FlagSerializer<CollisionMask>))]
    public int CollisionMask = (int) CollisionGroup.BulletImpassable;

    [DataField]
    public Color TargetedColor = Color.Green;

    [DataField]
    public Color DefaultColor = Color.Red;
}
