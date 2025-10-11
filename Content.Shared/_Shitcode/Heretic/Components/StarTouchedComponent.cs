using Robust.Shared.GameStates;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class StarTouchedComponent : Component
{
    [DataField]
    public float TickInterval = 0.2f;

    [DataField]
    public float Range = 8f;

    [DataField]
    public bool ApplyEffects;

    [ViewVariables(VVAccess.ReadWrite)]
    public float Accumulator;
}
