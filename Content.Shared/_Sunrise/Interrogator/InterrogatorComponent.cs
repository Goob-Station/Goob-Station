using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Serialization;

namespace Content.Shared._Sunrise.Interrogator;

[RegisterComponent]
public sealed partial class InterrogatorComponent : Component
{
    [ViewVariables]
    public ContainerSlot BodyContainer = default!;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float ExtractionTime = 30f;

    [ViewVariables]
    public float ExtractionProgress = 0;

    public EntityUid? PlayingStream;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("entryDelay")]
    public float EntryDelay = 2f;

    // SUNRISE-TODO: Более подходящий звук работы, мейби взять из сс13
    [DataField("extractingSound")]
    public SoundSpecifier ExtractingSound = new SoundPathSpecifier("/Audio/Machines/microwave_loop.ogg");

    [DataField("extractDoneSound")]
    public SoundSpecifier ExtractDoneSound = new SoundPathSpecifier("/Audio/_Sunrise/Interrogator/ding.ogg");

    [Serializable, NetSerializable]
    public enum InterrogatorVisuals : byte
    {
        ContainsEntity,
        IsOn
    }
}
