using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Chemistry.Components;
using Robust.Shared.Audio;

namespace Content.Pirate.Shared.BurnableByThermite;

/// <summary>
/// Component for structures, that allows thermite to burn trough them.
/// </summary>
[RegisterComponent]
public sealed partial class BurnableByThermiteComponent : Component
{
    /// <summary>
    /// Time it takes to burn through the structure since full ignition. In seconds.
    /// </summary>
    [DataField] public float BurnTime = 20f;
    /// <summary>
    /// Time it takes to fully ignite thermite. In seconds.
    /// </summary>
    [DataField] public float IgnitionTime = 2f;
    /// <summary>
    /// Damage per second dealt to the structure while burning.
    /// </summary>
    [DataField] public FixedPoint2 DPS = 12f;
    [DataField] public FixedPoint2 TotalDamageUntilMelting = 200f;
    [DataField] public SoundSpecifier BurningSound = new SoundPathSpecifier("/Audio/_Pirate/Effects/thermite_burning.ogg");
    public EntityUid? BurningSoundStream = null;

    [ViewVariables(VVAccess.ReadOnly)] public FixedPoint2 TotalDamageDealt = 0f;

    /// <summary>
    /// Represents the solution in structure that contains the thermite.
    /// Used to check if covered in thermite when trying to ignite.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)] public SolutionComponent? ThermiteSolutionComponent;
    /// <summary>
    /// Indicates if structure is ignited, but not fully burning yet.
    /// </summary>
    public bool Ignited = false;
    /// <summary>
    /// Indicates if structure is fully burning.
    /// </summary>
    public bool Burning = false;

    public float IgnitionStartTime;
    public float BurningStartTime;
}
