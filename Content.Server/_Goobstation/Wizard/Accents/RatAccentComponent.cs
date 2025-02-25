namespace Content.Server._Goobstation.Wizard.Accents;

[RegisterComponent]
public sealed partial class RatAccentComponent : AnimalAccentComponent
{
    public override List<LocId> AnimalNoises => new()
    {
        "accent-words-rat-1",
        "accent-words-rat-2",
        "accent-words-rat-3",
    };
}
