using Robust.Shared.GameObjects;

namespace Content.Goobstation.Shared.Critter.Components;

[RegisterComponent]
public sealed partial class BloodFeedingComponent : Component
{
    [DataField]
    public EntityUid Target = EntityUid.Invalid;

    /// <summary>
    /// Whether first bite already happened
    /// </summary>
    [DataField]
    public bool HasBitten = false;
}
