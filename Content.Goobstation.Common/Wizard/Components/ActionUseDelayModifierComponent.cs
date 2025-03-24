using System;
using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Goobstation.Common.Wizard.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ActionUseDelayModifierComponent : Component
{
    [DataField(required: true)]
    public TimeSpan? UseDelay;
}
