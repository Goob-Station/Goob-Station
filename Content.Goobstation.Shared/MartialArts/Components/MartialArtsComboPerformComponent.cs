namespace Content.Goobstation.Shared.MartialArts.Components;

// this is SLOP but entity conditions arent advanced enough to work for martial arts without this
[RegisterComponent]
public sealed partial class MartialArtsComboPerformedComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid PerformedOn;
}

[RegisterComponent]
public sealed partial class MartialArtsComboPerformedOnComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public Entity<ComboPerformerComponent> Performer;
};
