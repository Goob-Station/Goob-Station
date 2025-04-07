namespace Content.Server._Goobstation.Wizard.Accents;

public abstract partial class AnimalAccentComponent : Component
{
    [DataField]
    public virtual List<LocId> AnimalNoises { get; set; }

    [DataField]
    public virtual List<LocId> AnimalAltNoises { get; set; }

    [DataField]
    public virtual float AltNoiseProbability { get; set; }
}
