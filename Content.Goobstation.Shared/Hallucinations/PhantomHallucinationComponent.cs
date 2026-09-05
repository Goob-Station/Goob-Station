using Robust.Shared.Audio;

namespace Content.Goobstation.Shared.Hallucinations;

/// <summary>
/// A phantom mob conjured by a hallucination.
/// </summary>
[RegisterComponent]
public sealed partial class PhantomHallucinationComponent : Component
{
    [DataField]
    public EntityUid Victim;

    [DataField]
    public SoundSpecifier? AttackSound;

    /// <summary>
    /// How close the phantom must be to swing.
    /// </summary>
    [DataField]
    public float AttackRange = 1.5f;

    [DataField]
    public float AttackDelayMin = 1.5f;

    [DataField]
    public float AttackDelayMax = 3f;

    [DataField]
    public TimeSpan NextAttack;
}
