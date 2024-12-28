using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.WhiteDream.BloodCult.Items.BloodSpear;

[RegisterComponent]
public sealed partial class BloodSpearComponent : Component
{
    [DataField]
    public EntityUid? Master;

    [DataField]
    public TimeSpan ParalyzeTime = TimeSpan.FromSeconds(4);

    [DataField]
    public EntProtoId RecallActionId = "ActionBloodSpearRecall";

    public EntityUid? RecallAction;

    [DataField]
    public SoundSpecifier RecallAudio = new SoundPathSpecifier(
        new ResPath("/Audio/WhiteDream/BloodCult/rites.ogg"),
        AudioParams.Default.WithVolume(-3));
}
