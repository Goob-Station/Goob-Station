using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Blob.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BlobPodComponent : Component
{
    [AutoNetworkedField]
    [ViewVariables(VVAccess.ReadOnly)]
    public bool IsZombifying = false;

    [AutoNetworkedField]
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ZombifiedEntityUid = default!;

    [ViewVariables(VVAccess.ReadWrite), DataField("zombifyDelay")]
    public float ZombifyDelay = 5.00f;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? Core = null;

    [ViewVariables(VVAccess.ReadWrite), DataField("zombifySoundPath")]
    public SoundSpecifier ZombifySoundPath = new SoundPathSpecifier("/Audio/Effects/Fluids/blood1.ogg");

    [ViewVariables(VVAccess.ReadWrite), DataField("zombifyFinishSoundPath")]
    public SoundSpecifier ZombifyFinishSoundPath = new SoundPathSpecifier("/Audio/Effects/gib1.ogg");

    public Entity<AudioComponent>? ZombifyStingStream;
    public EntityUid? ZombifyTarget;

    [DataField]
    public ProtoId<TagPrototype> HostTag = "BlobMind";
}
