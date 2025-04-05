namespace Content.Goobstation.Server.Wizard.Accents;

[RegisterComponent]
public sealed partial class CowAccentComponent : AnimalAccentComponent
{
    public override List<LocId> AnimalNoises => new()
    {
        "accent-words-cow-1",
        "accent-words-cow-2",
        "accent-words-cow-3",
    };
}
