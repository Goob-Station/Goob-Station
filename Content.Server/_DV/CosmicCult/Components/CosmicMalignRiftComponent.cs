using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._DV.CosmicCult.Components;

[RegisterComponent]
[AutoGenerateComponentPause]
public sealed partial class CosmicMalignRiftComponent : Component
{
    [DataField]
    public bool Used;

    [DataField]
    public bool Occupied;

    // [DataField]
    // public EntProtoId GrailID = "NullRodGrail"; // Not implemented at this time

    [DataField]
    public TimeSpan AbsorbTime = TimeSpan.FromSeconds(20);

    [DataField]
    public TimeSpan MinPulseTime = TimeSpan.FromSeconds(10);

    [DataField]
    public TimeSpan MaxPulseTime = TimeSpan.FromSeconds(30);

    [DataField]
    public float PulseRange = 15f;
    /// <summary>
    /// The chance for each entity in range to be affected by a pulse
    /// </summary>
    [DataField]
    public float PulseProb = 0.75f;

    [AutoPausedField, DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextPulseTime = default!;

    [DataField]
    public EntProtoId PulseVFX = "CosmicGenericVFX";
}
