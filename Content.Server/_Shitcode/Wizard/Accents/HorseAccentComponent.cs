namespace Content.Server._Goobstation.Wizard.Accents;

[RegisterComponent]
public sealed partial class HorseAccentComponent : AnimalAccentComponent
{
    public override List<LocId> AnimalNoises => new()
    {
        "accent-words-horse-1",
        "accent-words-horse-2",
        "accent-words-horse-3",
        "accent-words-horse-4",
        "accent-words-horse-5",
    };
}
