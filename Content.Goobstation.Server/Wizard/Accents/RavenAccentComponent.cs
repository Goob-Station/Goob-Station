namespace Content.Server._Goobstation.Wizard.Accents;

[RegisterComponent]
public sealed partial class RavenAccentComponent : AnimalAccentComponent
{
    public override List<LocId> AnimalNoises => new()
    {
        "accent-words-raven-1",
        "accent-words-raven-2",
        "accent-words-raven-3",
    };

    public override List<LocId> AnimalAltNoises => new()
    {
        "accent-words-alt-raven-1",
    };

    public override float AltNoiseProbability => 0.01f;
}
