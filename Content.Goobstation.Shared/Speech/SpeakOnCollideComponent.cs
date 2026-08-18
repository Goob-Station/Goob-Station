using Content.Shared.Dataset;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Speech;

[RegisterComponent, NetworkedComponent]
public sealed partial class SpeakOnCollideComponent : Component
{
    /// <summary>
    /// The text to speak. This has priority over Pack.
    /// </summary>
    [DataField]
    public LocId? Text;

    /// <summary>
    /// The identifier for the dataset prototype containing messages to be spoken by this entity.
    /// The spoken text will be picked randomly from it.
    /// </summary>
    [DataField]
    public ProtoId<LocalizedDatasetPrototype>? Pack;
}
