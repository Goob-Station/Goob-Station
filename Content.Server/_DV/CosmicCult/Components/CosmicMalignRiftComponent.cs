using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.CosmicCult.Components;

[RegisterComponent]
public sealed partial class CosmicMalignRiftComponent : Component
{
    [DataField]
    public bool Used;

    [DataField]
    public bool Occupied;

    [DataField]
    public EntProtoId PurgeVFX = "CleanseEffectVFX";

    [DataField]
    public SoundSpecifier PurgeSound = new SoundPathSpecifier("/Audio/_DV/CosmicCult/cleanse_deconversion.ogg");

    // [DataField]
    // public EntProtoId GrailID = "NullRodGrail"; // Not implemented at this time

    [DataField]
    public TimeSpan BibleTime = TimeSpan.FromSeconds(35);

    [DataField]
    public TimeSpan ChaplainTime = TimeSpan.FromSeconds(20);

    [DataField]
    public TimeSpan AbsorbTime = TimeSpan.FromSeconds(35);
}
