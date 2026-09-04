using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Shredder;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShredderComponent : Component
{
    public TimeSpan FinishedShreddingTime = TimeSpan.Zero;

    [DataField]
    public TimeSpan ShreddingTime = TimeSpan.FromSeconds(6.3);

    [DataField]
    public string ShreddingState = "shredding";

    [DataField]
    public SoundPathSpecifier ShreddingSound = new ("/Audio/_Goobstation/Machines/Shredder/shredder.ogg");
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
