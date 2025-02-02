using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using System;

namespace Content.Shared.Disease;

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
    /// Genotypes we have gained immunity against from getting sick by them or having taken a vaccine for
    /// </summary>
    [DataField]
    public List<int> ImmuneTo = new();

    /// <summary>
    /// Whether to still work while dead
    /// </summary>
    [DataField]
    public bool InDead = false;
}
