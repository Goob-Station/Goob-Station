using JetBrains.Annotations;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.Cyberpsychosis;

public enum CyberSanitySymptom : byte
{
    Cough,
    Seizure,
    Rage,
    Buildup,
    Hallucination,
}

[DataDefinition]
public sealed partial class CyberSanityTier
{
    /// <summary>
    /// What value the effect starts at.
    /// </summary>
    [DataField]
    public float Threshold;

    /// <summary>
    /// What it will actually do.
    /// </summary>
    [DataField]
    public CyberSanityEffect[] Effects = [];
}

/// <summary>
/// The actual effect that happens.
/// </summary>
[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class CyberSanityEffect
{
    /// <summary>
    /// Continuous effects run every update.
    /// </summary>
    public virtual bool Continuous => false;

    /// <summary>
    /// The delay is used for things like rage, speach, etc.
    /// </summary>
    [DataField]
    public float DelayMin = 7f;

    [DataField]
    public float DelayMax = 40f;

    public abstract void Effect(EntityUid uid, CyberSanityComponent comp, IEntityManager entityManager, IRobustRandom random, float threshold);
}
