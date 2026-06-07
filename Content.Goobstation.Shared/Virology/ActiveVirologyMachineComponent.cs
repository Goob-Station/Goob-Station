using System;

namespace Content.Goobstation.Shared.Virology;

[RegisterComponent]
public sealed partial class ActiveVirologyMachineComponent : Component
{
    [ViewVariables]
    public TimeSpan EndTime;
}
