using System.Numerics;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Explosion;
using Content.Shared.Random;
using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wizard.Events;

public sealed partial class ScreamForMeEvent : EntityTargetActionEvent
{
    [DataField]
    public EntProtoId Effect = "SanguineFlashEffect";
}

public sealed partial class CorpseExplosionEvent : EntityTargetActionEvent
{
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
    public DamageSpecifier Damage = new();
}

public sealed partial class HomingToolboxEvent : WorldTargetActionEvent
{
    [DataField]
    public EntProtoId Proto = "ProjectileToolboxHoming";

    [DataField]
    public float ProjectileSpeed = 20f;
}

public sealed partial class MagicMissileEvent : InstantActionEvent
{
    [DataField]
    public EntProtoId Proto = "ProjectileMagicMissile";

    [DataField]
    public float Range = 7f;

    [DataField]
    public float ProjectileSpeed = 6f;
}

public sealed partial class BananaTouchEvent : EntityTargetActionEvent
{
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

public sealed partial class DisableTechEvent : InstantActionEvent
{
    [DataField]
    public float Range = 10f;

    [DataField]
    public float EnergyConsumption = 50000f;

    [DataField]
    public float DisableDuration = 60f;

    [DataField]
    public EntProtoId Effect = "EmpFlashEffect";
}

public sealed partial class SmokeSpellEvent : InstantActionEvent
{
    [DataField]
    public EntProtoId Proto = "Smoke";

    [DataField]
    public float Duration = 10;

    [DataField]
    public int SpreadAmount = 30;
}

public sealed partial class MimeMalaiseEvent : EntityTargetActionEvent
{
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

public sealed partial class ChuuniInvocationsEvent : InstantActionEvent
{
    [DataField]
    public Dictionary<string, EntProtoId> Gear = new()
    {
        {"eyes", "ClothingEyesEyepatchMedical"},
    };

    [DataField]
    public ProtoId<TagPrototype> WizardHatTag = "WizardHat";
}

public sealed partial class StopTimeEvent : InstantActionEvent
{
    [DataField]
    public EntProtoId Proto = "Chronofield";
}

public sealed partial class RathenEvent : InstantActionEvent
{
    [DataField]
    public float MaxRange = 5f;

    [DataField]
    public TimeSpan StunTime = TimeSpan.FromSeconds(5);

    [DataField]
    public DamageSpecifier SuperFartDamage = new()
    {
        DamageDict = { { "Blunt", 10 } },
    };

    [DataField]
    public float LimbTearChance = 0.2f;
}

public sealed partial class RepulseEvent : InstantActionEvent
{
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

public sealed partial class BlindSpellEvent : EntityTargetActionEvent
{
    [DataField]
    public TimeSpan BlindDuration = TimeSpan.FromSeconds(30f);

    [DataField]
    public TimeSpan BlurDuration = TimeSpan.FromSeconds(40f);

    [DataField]
    public EntProtoId? Effect = "GrenadeFlashEffect";
}

public sealed partial class PredictionToggleSpellEvent : EntityTargetActionEvent
{
    [DataField]
    public SoundSpecifier? Sound;
}

public sealed partial class LesserSummonGunsEvent : InstantActionEvent
{
    [DataField]
    public EntProtoId Proto = "WeaponBoltActionEnchanted";
}

public sealed partial class ArcaneBarrageEvent : InstantActionEvent
{
    [DataField]
    public EntProtoId Proto = "ArcaneBarrage";
}

public sealed partial class ThrownLightningEvent : InstantActionEvent
{
    [DataField]
    public EntProtoId Proto = "ThrownLightning";

    [DataField]
    public SoundSpecifier? Sound;
}

public sealed partial class TileToggleSpellEvent : EntityTargetActionEvent
{
    [DataField]
    public SoundSpecifier? Sound;
}

public sealed partial class SpellCardsEvent : WorldTargetActionEvent
{
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

public sealed partial class SummonSimiansEvent : InstantActionEvent
{
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
