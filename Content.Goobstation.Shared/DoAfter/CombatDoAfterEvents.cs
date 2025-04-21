using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.DoAfter;

[Serializable, NetSerializable]
public sealed partial class CombatDoAfterEvent : SimpleDoAfterEvent;

[ImplicitDataDefinitionForInheritors]
public abstract partial class BaseCombatDoAfterSuccessEvent : EntityEventArgs;

public abstract partial class CombatDoAfterMeleeHitEvent : BaseCombatDoAfterSuccessEvent
{
    public IReadOnlyList<EntityUid> Targets;

    public DamageSpecifier BonusDamage = new();
}

public sealed partial class CombatSyringeTriggerEvent : CombatDoAfterMeleeHitEvent
{
    [DataField]
    public SoundSpecifier? InjectSound = new SoundPathSpecifier("/Audio/_Goobstation/Weapons/Effects/pierce1.ogg");

    [DataField]
    public float SolutionSplitFraction = 1f;

    [DataField]
    public DamageSpecifier SyringeExtraDamage = new()
    {
        DamageDict =
        {
            { "Piercing", 4 },
        },
    };
}
