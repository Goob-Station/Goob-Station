using System.Numerics;
using Content.Shared.Actions;
using Content.Shared.Atmos;
using Content.Shared.Chemistry.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Destructible.Thresholds;
using Content.Shared.Explosion;
using Content.Shared.FixedPoint;
using Content.Shared.Magic;
using Content.Shared.Physics;
using Content.Shared.Polymorph;
using Content.Shared.Random;
using Content.Shared.Tag;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.Maths;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Goobstation.Wizard;

public sealed partial class CluwneCurseEvent : EntityTargetActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }

    [DataField]
    public TimeSpan ParalyzeDuration = TimeSpan.FromSeconds(5);

    [DataField]
    public TimeSpan StutterDuration = TimeSpan.FromSeconds(30);
}

public sealed partial class BananaTouchEvent : EntityTargetActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }

    [DataField]
    public Dictionary<string, EntProtoId> Gear = new()
    {
        {"mask", "ClothingMaskClown"},
        {"jumpsuit", "ClothingUniformJumpsuitClown"},
        {"shoes", "ClothingShoesClown"},
        {"id", "ClownPDA"},
    };

    [DataField]
    public TimeSpan ParalyzeDuration = TimeSpan.FromSeconds(5);

    [DataField]
    public TimeSpan JitterStutterDuration = TimeSpan.FromSeconds(30);
}

public sealed partial class MimeMalaiseEvent : EntityTargetActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }

    [DataField]
    public Dictionary<string, EntProtoId> Gear = new()
    {
        {"mask", "ClothingMaskMime"},
        {"jumpsuit", "ClothingUniformJumpsuitMime"},
        {"belt", "ClothingBeltSuspendersRed"},
        {"id", "MimePDA"},
    };

    [DataField]
    public TimeSpan WizardMuteDuration = TimeSpan.FromSeconds(15);

    [DataField]
    public TimeSpan ParalyzeDuration = TimeSpan.FromSeconds(5);
}

public sealed partial class MagicMissileEvent : InstantActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }

    [DataField]
    public EntProtoId Proto = "ProjectileMagicMissile";

    [DataField]
    public float Range = 7f;

    [DataField]
    public float ProjectileSpeed = 4.5f;
}

public sealed partial class DisableTechEvent : InstantActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }

    [DataField]
    public float Range = 10f;

    [DataField]
    public float EnergyConsumption = 50000f;

    [DataField]
    public float DisableDuration = 60f;

    [DataField]
    public EntProtoId Effect = "EmpFlashEffect";
}

public sealed partial class SmokeSpellEvent : InstantActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }

    [DataField]
    public EntProtoId Proto = "Smoke";

    [DataField]
    public float Duration = 10;

    [DataField]
    public int SpreadAmount = 30;
}

public sealed partial class RepulseEvent : InstantActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }

    [DataField]
    public float Force = 180f;

    [DataField]
    public float MinRange = 0.00001f;

    [DataField]
    public float MaxRange = 5f;

    [DataField]
    public TimeSpan StunTime = TimeSpan.FromSeconds(4);

    [DataField]
    public EntProtoId EffectProto = "EffectRepulse";
}

public sealed partial class StopTimeEvent : InstantActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }

    [DataField]
    public EntProtoId Proto = "Chronofield";
}

public sealed partial class CorpseExplosionEvent : EntityTargetActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }

    [DataField]
    public float TotalIntensity = 200f;

    [DataField]
    public float Slope = 1.5f;

    [DataField]
    public float MaxIntenity = 100f;

    [DataField]
    public float KnockdownRange = 4f;

    [DataField]
    public TimeSpan SiliconStunTime = TimeSpan.FromSeconds(6f);

    [DataField]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(4f);

    [DataField]
    public ProtoId<ExplosionPrototype> ExplosionId = "Corpse";

    [DataField(required: true)]
    public DamageSpecifier Damage;
}

public sealed partial class BlindSpellEvent : EntityTargetActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }

    [DataField]
    public TimeSpan BlindDuration = TimeSpan.FromSeconds(30f);

    [DataField]
    public TimeSpan BlurDuration = TimeSpan.FromSeconds(40f);

    [DataField]
    public EntProtoId? Effect = "GrenadeFlashEffect";
}

public sealed partial class BindSoulEvent : InstantActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }

    [DataField]
    public EntityWhitelist Blacklist;

    [DataField]
    public EntProtoId Entity = "MobSkeletonPerson";

    [DataField]
    public SoundSpecifier? Sound;

    [DataField]
    public Dictionary<string, EntProtoId> Gear = new()
    {
        {"head", "ClothingHeadHatBlackwizardReal"},
        {"outerClothing", "ClothingOuterWizardBlackReal"},
    };
}

public sealed partial class PolymorphSpellEvent : InstantActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }

    [DataField]
    public ProtoId<PolymorphPrototype>? ProtoId;

    [DataField]
    public bool MakeWizard = true;

    [DataField]
    public SoundSpecifier? Sound;

    [DataField]
    public bool LoadActions;
}

public sealed partial class MutateSpellEvent : InstantActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }

    [DataField]
    public float Duration = 30f;
}

public sealed partial class TeslaBlastEvent : InstantActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }

    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(10);

    [DataField]
    public float Range = 7f;

    [DataField]
    public int BoltCount = 1;

    [DataField]
    public int ArcDepth = 5;

    [DataField]
    public Vector2 MinMaxDamage = new(15f, 60f);

    [DataField]
    public Vector2 MinMaxStunTime = new(2f, 8f);

    [DataField]
    public EntProtoId LightningPrototype = "SuperchargedLightning";

    [DataField]
    public EntProtoId EffectPrototype = "EffectElectricity";

    [DataField]
    public SoundSpecifier? Sound;
}

public sealed partial class LightningBoltEvent : EntityTargetActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }

    [DataField]
    public float Damage = 40f;

    [DataField]
    public EntProtoId Proto = "ChargedLightning";
}

public sealed partial class HomingToolboxEvent : EntityWorldTargetActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }

    [DataField]
    public EntProtoId Proto = "ProjectileToolboxHoming";

    [DataField]
    public float ProjectileSpeed = 20f;
}

public sealed partial class SpellCardsEvent : EntityWorldTargetActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }

    [DataField]
    public EntProtoId RedProto = "ProjectileSpellCardRed";

    [DataField]
    public EntProtoId PurpleProto = "ProjectileSpellCardPurple";

    [DataField]
    public float ProjectileSpeed = 20f;

    [DataField]
    public int ProjectilesAmount = 7;

    [DataField]
    public Angle Spread = Angle.FromDegrees(30);

    [DataField]
    public float MaxAngularVelocity = MathF.PI / 3f;

    [DataField]
    public Vector2 MinMaxLinearDamping = new(3f, 7f);
}

public sealed partial class ArcaneBarrageEvent : InstantActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }

    [DataField]
    public EntProtoId Proto = "ArcaneBarrage";
}

public sealed partial class LesserSummonGunsEvent : InstantActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }

    [DataField]
    public EntProtoId Proto = "WeaponBoltActionEnchanted";
}

public sealed partial class BarnyardCurseEvent : EntityTargetActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }

    [DataField(required: true)]
    public Dictionary<EntProtoId, SoundSpecifier?> Masks = new();

    [DataField]
    public ProtoId<TagPrototype> CursedMaskTag = "CursedAnimalMask";
}

public sealed partial class ScreamForMeEvent : EntityTargetActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }

    [DataField]
    public EntProtoId Effect = "SanguineFlashEffect";
}

public sealed partial class InstantSummonsEvent : InstantActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }

    [DataField]
    public SoundSpecifier? SummonSound;
}

public sealed partial class WizardTeleportEvent : InstantActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }
}

public sealed partial class TrapsSpellEvent : InstantActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }

    [DataField]
    public List<EntProtoId> Traps = new()
    {
        "TrapShock",
        "TrapFlame",
        "TrapDamage",
        "TrapChill",
        "TrapBlind",
    };

    [DataField]
    public float Range = 3f;

    [DataField]
    public int Amount = 5;
}

public sealed partial class SummonMobsEvent : InstantActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }

    [DataField]
    public List<EntProtoId> Mobs = new();

    [DataField]
    public float Range = 1f;

    [DataField]
    public int Amount = 9;

    [DataField]
    public Angle SpawnAngle = Angle.FromDegrees(160);

    [DataField(customTypeSerializer: typeof(FlagSerializer<CollisionMask>))]
    public int CollisionMask = (int) CollisionGroup.MobMask;

    [DataField]
    public bool FactionIgnoreSummoner;
}

public sealed partial class SummonSimiansEvent : InstantActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }

    [DataField(required: true)]
    public ProtoId<WeightedRandomEntityPrototype> Mobs;

    [DataField(required: true)]
    public ProtoId<WeightedRandomEntityPrototype> Weapons;

    [DataField]
    public float Range = 1f;

    [DataField]
    public int Amount = 4;

    [DataField]
    public Angle SpawnAngle = Angle.FromDegrees(40);
}

public sealed partial class ExsanguinatingStrikeEvent : InstantActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }
}

public sealed partial class ChuuniInvocationsEvent : InstantActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }

    [DataField]
    public Dictionary<string, EntProtoId> Gear = new()
    {
        {"eyes", "ClothingEyesEyepatchMedical"},
    };

    [DataField]
    public ProtoId<TagPrototype> WizardHatTag = "WizardHat";
}

public sealed partial class SwapSpellEvent : EntityTargetActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }

    [DataField]
    public SoundSpecifier? Sound;

    [DataField]
    public float Range = 15f;

    [DataField]
    public EntProtoId Effect = "SwapSpellEffect";
}

public sealed partial class SoulTapEvent : InstantActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }

    [DataField]
    public FixedPoint2 MaxHealthReduction = 20;

    [DataField]
    public ProtoId<DamageTypePrototype> KillDamage = "Cellular";

    [DataField]
    public ProtoId<TagPrototype> DeadTag = "SoulTapped";
}

public sealed partial class ThrownLightningEvent : InstantActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }

    [DataField]
    public EntProtoId Proto = "ThrownLightning";

    [DataField]
    public SoundSpecifier? Sound;
}

public sealed partial class ChargeMagicEvent : InstantActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }

    [DataField]
    public ProtoId<TagPrototype> WandTag = "WizardWand";

    [DataField]
    public float WandChargeRate = 1000f;

    [DataField]
    public float MinWandDegradeCharge = 500f;

    [DataField]
    public float WandDegradePercentagePerCharge = 0.5f;

    [DataField]
    public List<ProtoId<TagPrototype>> RechargeTags = new()
    {
        "WizardWand",
        "WizardStaff",
    };
}

public sealed partial class BlinkSpellEvent : InstantActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }

    [DataField]
    public MinMax Radius = new(0, 6);
}

[DataDefinition]
public sealed partial class SummonSimiansMaxedOutEvent : EntityEventArgs
{
    [DataField]
    public EntProtoId Action = "ActionGorillaForm";

    [DataField]
    public ProtoId<TagPrototype> MaxLevelTag = "SummonSimiansMaxLevelAction";

    [DataField]
    public ProtoId<TagPrototype> GorillaFormTag = "GorillaFormAction";

    [DataField]
    public Color MessageColor = Color.FromHex("#EDC349");
}

[DataDefinition]
public sealed partial class SummonGhostsEvent : EntityEventArgs
{
    [DataField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/_Goobstation/Wizard/ghost2.ogg");
}

[DataDefinition]
public sealed partial class DimensionShiftEvent : EntityEventArgs
{
    [DataField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/_Goobstation/Wizard/ghost.ogg");

    [DataField]
    public float OxygenMoles = 10f;

    [DataField]
    public float NitrogenMoles = 10f;

    [DataField]
    public float CarbonDioxideMoles = 10f;

    [DataField]
    public float Temperature = Atmospherics.T20C;

    [DataField]
    public string? Parallax = "Wizard";
}

[DataDefinition]
public sealed partial class RandomizeSpellsEvent : EntityEventArgs
{
    [DataField]
    public float TotalBalance = 10;

    [DataField(required: true)]
    public Dictionary<ProtoId<WeightedRandomEntityPrototype>, int?> SpellsDict;
}
