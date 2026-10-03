using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Mech.Components;

/// <summary>
/// Rushes the mech forwards and deals damage to structures / players.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class MechRamComponent : Component
{
    [DataField]
    public EntProtoId ActionProto = "ActionMechRam";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;

    [DataField]
    public SpriteSpecifier CooldownIcon = new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Interface/Actions/mech.rsi"), "mech_charge_cooldown");

    [DataField]
    public SpriteSpecifier ReadyIcon = new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Interface/Actions/mech.rsi"), "mech_charge_off");

    [ViewVariables, AutoNetworkedField]
    public bool ShowingCooldownIcon;

    [DataField]
    public int LungeDistance = 6;


    [DataField]
    public TimeSpan TileStepInterval = TimeSpan.FromSeconds(0.09);

    [DataField]
    public DamageSpecifier StructureDamage = new()
    {
        DamageDict = new() { { "Structural", 450 }, { "Blunt", 30 } },
    };

    [DataField]
    public DamageSpecifier MobDamage = new()
    {
        DamageDict = new() { { "Blunt", 20 } },
    };

    [DataField]
    public DamageSpecifier SelfDamage = new()
    {
        DamageDict = new() { { "Blunt", 15 } },
    };

    [DataField]
    public TimeSpan MobKnockdownTime = TimeSpan.FromSeconds(3);

    [DataField]
    public float ThrowSpeed = 8f;

    [DataField]
    public FixedPoint2 PowerCageEnergyCost = 60;

    [DataField]
    public SoundSpecifier StartSound = new SoundPathSpecifier("/Audio/Mecha/sound_mecha_hydraulic.ogg");

    [DataField]
    public SoundSpecifier StepSound = new SoundPathSpecifier("/Audio/Mecha/sound_mecha_powerloader_step.ogg");

    [DataField]
    public SoundSpecifier ImpactSound = new SoundCollectionSpecifier("MetalSlam");

    [DataField]
    public SoundSpecifier SmashSound = new SoundCollectionSpecifier("MetalBreak");


    [DataField]
    public float ImpactCameraKick = 3f;

    [ViewVariables, AutoNetworkedField]
    public bool Active;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan NextStep;

    [ViewVariables, AutoNetworkedField]
    public int StepsLeft;

    [ViewVariables, AutoNetworkedField]
    public HashSet<EntityUid> Trampled = new();
}
