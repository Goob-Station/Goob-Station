using Content.Server._Goobstation.Blob;
using Content.Shared._Goobstation.Blob.Components;
using Content.Shared.Mind;
using Robust.Shared.Audio;

namespace Content.Server.GameTicking.Rules.Components;

[RegisterComponent, Access(typeof(BlobRuleSystem), typeof(BlobCoreSystem), typeof(BlobObserverSystem))]
public sealed partial class BlobRuleComponent : Component
{
    [DataField]
    public SoundSpecifier? AlertAudio = new SoundPathSpecifier("/Audio/Announcements/outbreak5.ogg");

    [ViewVariables]
    public List<(EntityUid mindId, MindComponent mind)> Blobs = new(); //BlobRoleComponent

    [ViewVariables]
    public BlobStage Stage = BlobStage.Default;

    [ViewVariables]
    public float Accumulator = 0f;
}


public enum BlobStage : byte
{
    Default,
    Begin,
    Critical,
    TheEnd,
}
