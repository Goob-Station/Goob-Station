using System.Numerics;
using Content.Shared.Actions;
using Content.Shared.Atmos;
using Content.Shared.Damage;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.ModSuits.Modules;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ModSphereTransformModuleComponent : Component
{
    [DataField]
    public Color PrimaryColor = Color.FromHex("#302f33");

    [DataField]
    public Color DarkColor = Color.FromHex("#181719");

    [DataField]
    public Color AccentColor = Color.FromHex("#a4fef4");

    [DataField]
    public float Radius = 0.35f;

    [DataField]
    public float Acceleration = 7.2f;

    [DataField]
    public float MaxSpeed = 7.8f;

    [DataField]
    public float RollingResistance = 1.6f;

    [DataField]
    public float BrakeDeceleration = 7f;

    [DataField]
    public float BrakeRadius = 0.6f;

    [DataField]
    public float FullThrustDistance = 3f;

    [DataField]
    public float MinThrust = 0.3f;

    [DataField]
    public float Restitution = 0.45f;

    [DataField]
    public float BounceThreshold = 1f;

    [DataField]
    public float ImpactSoundThreshold = 2.5f;

    [DataField]
    public float KnockdownSpeed = 4f;

    [DataField]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(2);

    [DataField]
    public float KnockdownSpeedMultiplier = 0.8f;

    [DataField]
    public float ExplosionCoefficient = 0.8f;

    [DataField]
    public SoundSpecifier ActivateSound = new SoundPathSpecifier("/Audio/_Goobstation/Items/Modsuit/ballin.ogg");

    [DataField]
    public SoundSpecifier DeactivateSound = new SoundPathSpecifier("/Audio/_Goobstation/Items/Modsuit/ballout.ogg");

    [DataField]
    public SoundSpecifier ImpactSound = new SoundCollectionSpecifier("MetalThud", AudioParams.Default.WithVariation(0.1f).WithVolume(-2f));

    [DataField]
    public SoundSpecifier KnockdownSound = new SoundPathSpecifier("/Audio/Effects/hit_kick.ogg");

    [DataField]
    public EntProtoId MissileAction = "ActionModSphereMissiles";

    [DataField, AutoNetworkedField]
    public EntityUid? MissileActionEntity;

    [DataField, AutoNetworkedField]
    public EntityUid? FormOwner;

    [DataField]
    public EntProtoId MissileProjectile = "BulletModMiningMissile";

    [DataField]
    public int MissileCount = 3;

    [DataField]
    public TimeSpan MissileInterval = TimeSpan.FromSeconds(0.2);

    [DataField]
    public float MissileSpeed = 20f;

    [DataField]
    public float MissilePower = 30f;

    [DataField]
    public float MissileMaxPressure = Atmospherics.OneAtmosphere * 0.5f;

    [DataField]
    public float MissileHomingRange = 0.75f;

    [DataField]
    public SoundSpecifier MissileDryFireSound = new SoundPathSpecifier("/Audio/Weapons/Guns/Empty/empty.ogg");

    [DataField]
    public SoundSpecifier MissileSound = new SoundPathSpecifier("/Audio/Weapons/Guns/Gunshots/rocket_launcher.ogg", AudioParams.Default.WithVolume(-6f));

    [ViewVariables]
    public int PendingMissiles;

    [ViewVariables]
    public TimeSpan NextMissile;

    [ViewVariables]
    public MapCoordinates MissileTarget;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ModMiningMissileComponent : Component
{
    [DataField]
    public float FaunaMultiplier = 4f;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ModSphereDrillComponent : Component
{
    [DataField]
    public DamageSpecifier Damage = new();

    [DataField]
    public float MinSpeed = 3f;

    [DataField]
    public float SpeedCost = 1.4f;

    [DataField]
    public float PowerCost = 5f;

    [DataField]
    public EntityWhitelist? Whitelist;

    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/_Goobstation/Items/Modsuit/sonic_jackhammer.ogg", AudioParams.Default.WithVolume(-4f));
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ModSphereFormComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Module;

    [DataField, AutoNetworkedField]
    public EntityUid? Drill;

    [DataField, AutoNetworkedField]
    public Color PrimaryColor;

    [DataField, AutoNetworkedField]
    public Color DarkColor;

    [DataField, AutoNetworkedField]
    public Color AccentColor;

    [DataField, AutoNetworkedField]
    public Vector2 SteerOffset;

    [DataField, AutoNetworkedField]
    public bool Thrusting;

    [DataField, AutoNetworkedField]
    public List<EntityUid> Hits = new();

    [DataField, AutoNetworkedField]
    public TimeSpan LastImpact;

    [DataField, AutoNetworkedField]
    public float ImpactStrength;

    [DataField, AutoNetworkedField]
    public Vector2 ImpactDirection;

    [ViewVariables]
    public Vector2 PreSolveVelocity;
}

[Serializable, NetSerializable]
public sealed class ModSphereSteerEvent : EntityEventArgs
{
    public Vector2 Offset;
    public bool Thrusting;
}

public sealed partial class ModSphereMissileActionEvent : WorldTargetActionEvent;
