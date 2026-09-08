using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Shredder;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ShredderComponent : Component
{
    [AutoNetworkedField]
    public TimeSpan FinishedShreddingTime = TimeSpan.Zero;

    [DataField]
    public TimeSpan ShreddingTime = TimeSpan.FromSeconds(6.3);

    [DataField]
    public string ShreddingState = "shredding";

    [DataField]
    public SoundSpecifier? ShreddingSound = new SoundPathSpecifier("/Audio/_Goobstation/Machines/Shredder/shredder.ogg");

    public ContainerSlot? Container;

    [ViewVariables, AutoNetworkedField]
    public EntityUid? StoredEntity;
}

[Serializable, NetSerializable]
public enum ShredderVisuals : byte
{
    VisualState,
}

[Serializable, NetSerializable]
public enum ShredderVisualsState : byte
{
    Normal,
    Shredding,
}
