using Robust.Shared.GameStates;

namespace Content.Goobstation.Common.Wizard;

[RegisterComponent, NetworkedComponent]
public sealed partial class ActionUseDelayModifierComponent : Component
{
    [DataField(required: true)]
    public TimeSpan? UseDelay;
}
