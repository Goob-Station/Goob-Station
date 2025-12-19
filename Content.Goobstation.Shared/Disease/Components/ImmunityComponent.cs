using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Disease.Components;

/// <summary>
/// For entities that have the ability to naturally fight back diseases
/// If you want to make some sort of alternate immunity of your own, copypaste and adjust SharedDiseaseSystem.Immunity.cs
/// </summary>
[RegisterComponent]
public sealed partial class ImmunityComponent : Component
{
    /// <summary>
    /// How fast this organism increases immune progress on diseases, per second
    /// </summary>
    [DataField]
    public float ImmunityGainRate = 0.002f;

    /// <summary>
    /// How fast this organism decreases infection progress at full immunity progress
    /// </summary>
    [DataField]
    public float ImmunityStrength = 0.02f;

    /// <summary>
    /// Which disease types can this affect the immunity strength against and gain immunity to
    /// </summary>
    [DataField]
    public HashSet<ProtoId<DiseaseTypePrototype>> AffectedTypes = new();

    /// <summary>
    /// Genotypes we have gained immunity against from getting sick by them or having taken a vaccine for
    /// </summary>
    [DataField]
    public HashSet<int> ImmuneTo = new();

    /// <summary>
    /// Whether to still work while dead
    /// </summary>
    [DataField]
    public bool InDead = false;
}
