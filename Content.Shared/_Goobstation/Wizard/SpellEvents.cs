using Content.Shared.Actions;
using Content.Shared.Chemistry.Components;
using Content.Shared.Damage;
using Content.Shared.Explosion;
using Content.Shared.Magic;
using Content.Shared.Roles;
using Content.Shared.Singularity.EntitySystems;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Wizard;

public sealed partial class CluwneCurseEvent : EntityTargetActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }

    [DataField]
    public TimeSpan ParalyzeDuration = TimeSpan.FromSeconds(5);

    [DataField]
    public TimeSpan JitterStutterDuration = TimeSpan.FromSeconds(30);
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

    [DataField]
    public Solution Solution = new("ThickSmoke", 50);
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
    public EntProtoId? EffectProto = "EffectRepulse";
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
    public TimeSpan BlindDuration = TimeSpan.FromSeconds(20f);

    [DataField]
    public TimeSpan BlurDuration = TimeSpan.FromSeconds(40f);

    [DataField]
    public EntProtoId Effect = "GrenadeFlashEffect";
}
