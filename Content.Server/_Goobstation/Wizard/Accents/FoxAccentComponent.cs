namespace Content.Server._Goobstation.Wizard.Accents;

[RegisterComponent]
public sealed partial class FoxAccentComponent : AnimalAccentComponent
{
    public override List<LocId> AnimalNoises => new()
    {
        "accent-words-fox-1",
        "accent-words-fox-2",
        "accent-words-fox-3",
        "accent-words-fox-4",
        "accent-words-fox-5",
    };
}
