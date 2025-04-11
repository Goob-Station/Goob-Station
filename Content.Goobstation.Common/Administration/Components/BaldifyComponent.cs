using Robust.Shared.GameStates;

namespace Content.Goobstation.Common.Administration.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class BaldifyComponent : Component
{
    [DataField]
    public string TargetLayer = "human_hair.rsi";

    public int? TargetIndex;
}