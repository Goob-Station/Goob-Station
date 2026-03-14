using Content.Shared.Body.Part;

namespace Content.Goobstation.Shared.Devil.Components;

[RegisterComponent]
public sealed partial class NoLimbForYouComponent : Component
{
    [DataField]
    public HashSet<string> ForbiddenSlots = new();

    [DataField]
    public float CheckInterval = 10f;

    public float Accumulator;
}
