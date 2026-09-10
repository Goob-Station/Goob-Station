namespace Content.Goobstation.Shared.Cyberpsychosis;

/// <summary>
/// Sanity meter that's exclusive to cybernetics.
/// This is essentially a placeholder right now.
/// TODO: Cyberpsychosis, add symptom effects.
/// </remarks>
[RegisterComponent]
public sealed partial class CyberSanityComponent : Component
{
    #region Meter
    [DataField]
    public int MaxSanity = 2000;

    [DataField]
    public float Sanity = 2000f;

    /// <summary>
    /// Sanity regeneration every second. This is meant to be low.
    /// Every cybernetic is supposed to get some sort of drain for example
    /// mantis blades get 2 passive, cyber eyes get 1 passive, etc so you
    /// could have 2 mantis blades + eyes which would only drain 6 sanity
    /// per second while inactive essentially meaning you get 4 sanity
    /// regen per second.
    /// </summary>
    [DataField]
    public float Regen = 10f;

    #endregion

    #region Symptoms

    /// <summary>
    /// Symptoms / thresholds. Currently placeholders.
    /// </summary>
    [DataField]
    public Dictionary<CyberSanitySymptom, CyberSanityTier> Tiers = new()
    {
        { CyberSanitySymptom.Cough, new() { Threshold = 1200 } },
        { CyberSanitySymptom.Buildup, new() { Threshold = 800 } },
        { CyberSanitySymptom.Hallucination, new() { Threshold = 600 } },
        { CyberSanitySymptom.Seizure, new() { Threshold = 500 } },
        { CyberSanitySymptom.Rage, new() { Threshold = 350 } },
    };

    [DataField]
    public TimeSpan NextSymptom;

    #endregion

    [DataField]
    public TimeSpan NextUpdate;

    [DataField]
    public float Rate;

    [DataField]
    public float Decay;

    [DataField]
    public float ImmunoblockerUnits;
}
