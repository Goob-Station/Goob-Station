using Content.Goobstation.Shared.Animations.API;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Animations;

[Prototype]
public sealed partial class AnimationPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public List<AnimationTrackData> Tracks = new();

    /// <summary>
    ///     Total animation length in seconds. Will fully stop animation on ends.
    /// </summary>
    [DataField(required: true)]
    public float Length = 0;
}
