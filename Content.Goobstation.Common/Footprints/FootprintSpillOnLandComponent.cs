using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Goobstation.Common.Footprints;

[RegisterComponent, NetworkedComponent]
public sealed partial class FootprintSpillOnLandComponent : Component
{
    [DataField]
    public float Alpha = 0.8f;

    [DataField]
    public bool DeleteEntity = true;

    [DataField]
    public ResPath Sprite = new("Fluids/splatter.rsi");

    [DataField]
    public List<string> States = new()
    {
        "splatter-0",
        "splatter-1",
        "splatter-2",
        "splatter-3",
        "splatter-4",
        "splatter-5",
    };
}
