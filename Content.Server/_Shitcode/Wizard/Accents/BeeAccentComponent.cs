namespace Content.Server._Goobstation.Wizard.Accents;

[RegisterComponent]
public sealed partial class BeeAccentComponent : AnimalAccentComponent
{
    public override List<LocId> AnimalNoises => new()
    {
        "accent-words-bee-1",
        "accent-words-bee-2",
        "accent-words-bee-3",
        "accent-words-bee-4",
    };
}
