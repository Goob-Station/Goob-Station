using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Wizard;

[RegisterComponent, NetworkedComponent]
public sealed partial class ActionUseDelayModifierComponent : Component
{
    [DataField(required: true)]
    public TimeSpan? UseDelay;
}
