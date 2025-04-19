using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.DoAfter;

[Serializable, NetSerializable]
public sealed partial class CombatDoAfterEvent : SimpleDoAfterEvent;

[ImplicitDataDefinitionForInheritors]
public abstract partial class BaseCombatDoAfterSuccessEvent : EntityEventArgs
{
    public IReadOnlyList<EntityUid>? AffectedEntities = null;
}

public sealed partial class CombatSyringeTriggerEvent : BaseCombatDoAfterSuccessEvent
{
    [DataField]
    public SoundSpecifier? InjectSound = new SoundPathSpecifier("/Audio/Weapons/pierce.ogg");
}
