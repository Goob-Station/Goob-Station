using Content.Shared._Shitmed.Autodoc.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Shitmed.Autodoc.Components;

/// <summary>
/// Component added while operating on a patient.
/// Only usable serverside, only thing that can be predicted is a surgery being active.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedAutodocSystem))]
[AutoGenerateComponentPause]
public sealed partial class ActiveAutodocComponent : Component
{
    /// <summary>
    /// The program index that is being used.
    /// <c>AutodocComponent.Programs</c> is frozen while an autodoc is active.
    /// </summary>
    [DataField]
    public int CurrentProgram;

    /// <summary>
    /// Index of the current program's step it is trying to do.
    /// </summary>
    [DataField]
    public int ProgramStep;

    /// <summary>
    /// Whether a step is waiting on a doafter to complete.
    /// </summary>
    [DataField]
    public bool Waiting;

    /// <summary>
    /// The current body, part and surgery being done, if any.
    /// </summary>
    [DataField]
    public (EntityUid, EntityUid, EntProtoId)? CurrentSurgery;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;
}
