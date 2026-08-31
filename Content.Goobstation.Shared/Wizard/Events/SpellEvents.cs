using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Explosion;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wizard.Systems;

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