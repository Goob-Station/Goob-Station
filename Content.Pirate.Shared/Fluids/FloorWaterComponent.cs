using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Pirate.Shared.Fluids;

/// <summary>
/// A body of standing water (e.g. <c>FloorWaterEntity</c>). Wading into it flows water over the mob:
/// worn clothing is fully soaked and its stains washed off (the dirty runoff drains away, not pooled).
/// It also acts like a drain: any liquid puddled on its own tile is absorbed.
/// </summary>
[RegisterComponent]
public sealed partial class FloorWaterComponent : Component
{
    /// <summary>
    /// Water flowed over a mob each time it wades in. Large enough to fully soak a full outfit and
    /// rinse its stains; the excess and washed-out stains drain away rather than pooling.
    /// </summary>
    [DataField]
    public FixedPoint2 ImmersionFlow = FixedPoint2.New(200);

    /// <summary>How often, in seconds, it absorbs puddles sitting on its tile.</summary>
    [DataField]
    public float AbsorbInterval = 1f;

    [ViewVariables]
    public float AbsorbAccumulator;
}
