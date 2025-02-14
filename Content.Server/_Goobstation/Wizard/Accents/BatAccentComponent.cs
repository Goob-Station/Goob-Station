namespace Content.Server._Goobstation.Wizard.Accents;

[RegisterComponent]
public sealed partial class BatAccentComponent : AnimalAccentComponent
{
    public override List<LocId> AnimalNoises => new()
    {
        "accent-words-mouse-1",
        "accent-words-mouse-2",
        "accent-words-mouse-3",
        "accent-words-mouse-4",
        "accent-words-mouse-5",
        "accent-words-mouse-6",
        "accent-words-mouse-7",
    };
}
