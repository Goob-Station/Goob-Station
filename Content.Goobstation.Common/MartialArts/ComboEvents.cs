namespace Content.Goobstation.Common.MartialArts;

public sealed class SaveLastAttacksEvent : EntityEventArgs;

public sealed class ResetLastAttacksEvent(bool dirty = true) : EntityEventArgs
{
    public bool Dirty = dirty;
}

public sealed class LoadLastAttacksEvent(bool dirty = true) : EntityEventArgs
{
    public bool Dirty = dirty;
}

[ByRefEvent]
public record struct GetPerformedAttackTypesEvent(List<ComboAttackType>? AttackTypes = null);
