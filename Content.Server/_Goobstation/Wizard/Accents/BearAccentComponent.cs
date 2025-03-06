namespace Content.Server._Goobstation.Wizard.Accents;

[RegisterComponent]
public sealed partial class BearAccentComponent : AnimalAccentComponent
{
    public override List<LocId> AnimalNoises => new()
    {
        "accent-words-bear-1",
        "accent-words-bear-2",
        "accent-words-bear-3",
        "accent-words-bear-4",
    };
}
