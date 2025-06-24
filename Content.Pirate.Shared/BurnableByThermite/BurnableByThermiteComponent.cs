using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Audio;

namespace Content.Pirate.Shared.BurnableByThermite;

/// <summary>
/// Component for structures, that allows thermite to burn trough them.
/// </summary>
[RegisterComponent]
public sealed partial class BurnableByThermiteComponent : Component
{
    /// <summary>
    /// Minimal time that thermite will burn. Burning won't stop even if <see cref="TotalDamageDealt"/> is larger that <see cref="TotalDamageUntilMelting"/>
    /// In seconds.
    /// </summary>
    [DataField] public float BurnTime = 25f;
    /// <summary>
    /// Time it takes to fully ignite thermite. In seconds.
    /// </summary>
    [DataField] public float IgnitionTime = 5f;
    /// <summary>
    /// Damage per second dealt to the structure while burning.
    /// </summary>
    [DataField] public FixedPoint2 DPS = 10f;
    [DataField] public FixedPoint2 TotalDamageUntilMelting = 200f;
    [DataField] public SoundSpecifier BurningSound = new SoundPathSpecifier("/Audio/_Pirate/Effects/thermite_burning.ogg");
    /// <summary>
    /// The amount of thermite needed to cover the structure. In units.
    /// </summary>
    [DataField] public float ThermiteAmount = 10f;
    public EntityUid? BurningSoundStream = null;

    [ViewVariables(VVAccess.ReadOnly)] public FixedPoint2 TotalDamageDealt = 0f;

    /// <summary>
    /// Represents the solution in structure that contains the thermite.
    /// Used to check if covered in thermite when trying to ignite.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)] public bool CoveredInThermite = false;
    /// <summary>
    /// Indicates if structure is ignited, but not fully burning yet.
    /// </summary>
    public bool Ignited = false;
    /// <summary>
    /// Indicates if structure is fully burning.
    /// </summary>
    public bool Burning = false;

    public double IgnitionStartTime;
    public double BurningStartTime;
}
