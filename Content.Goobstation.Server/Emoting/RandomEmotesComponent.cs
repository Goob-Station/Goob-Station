using Content.Shared.Random;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Emoting;

/// <summary>
/// Allows the entity to pick random emotes to say in the chat.
/// Tied to work with
/// </summary>
[RegisterComponent]
public sealed partial class RandomEmotesComponent : Component
{
    [DataField(required: true)]
    public ProtoId<WeightedRandomPrototype> Weights;
}
