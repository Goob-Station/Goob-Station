using Content.Shared.Atmos;
using Content.Shared.Physics;
using Robust.Shared.GameStates;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Utility;

namespace Content.Shared._Goobstation.Wizard.Traps;

[RegisterComponent, NetworkedComponent]
public sealed partial class IceCubeComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public BodyType? OldBodyType = null;

    [DataField]
    public BodyType FrozenBodyType = BodyType.Dynamic;

    [DataField]
    public float VelocityMultiplier = 0.2f;

    [DataField]
    public float TileFriction = 0.01f;

    [DataField]
    public float Restitution = 0.8f;

    [DataField]
    public float FrozenTemperature = Atmospherics.T0C - 200f;

    [DataField]
    public float UnfreezeTemperatureThreshold = Atmospherics.T0C;

    [DataField]
    public float UnfrozenTemperature = Atmospherics.T0C - 100f;

    [DataField]
    public float TemperaturePerHeatDamageIncrease = 5f;

    [DataField(customTypeSerializer: typeof(FlagSerializer<CollisionMask>))]
    public int CollisionMask = (int) CollisionGroup.FullTileMask;

    [DataField(customTypeSerializer: typeof(FlagSerializer<CollisionLayer>))]
    public int CollisionLayer = (int) CollisionGroup.WallLayer;

    [DataField]
    public TimeSpan BreakFreeDelay = TimeSpan.FromSeconds(10);

    [DataField]
    public SpriteSpecifier Sprite =
        new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Wizard/Effects/effects.rsi"), "ice_cube");
}

public enum IceCubeKey : byte
{
    Key,
}
