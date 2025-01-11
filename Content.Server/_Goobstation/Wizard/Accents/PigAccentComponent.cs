namespace Content.Server._Goobstation.Wizard.Accents;

[RegisterComponent]
public sealed partial class PigAccentComponent : AnimalAccentComponent
{
    public override List<LocId> AnimalNoises => new()
    {
        "accent-words-pig-1",
        "accent-words-pig-2",
        "accent-words-pig-3",
        "accent-words-pig-4",
    };
}
