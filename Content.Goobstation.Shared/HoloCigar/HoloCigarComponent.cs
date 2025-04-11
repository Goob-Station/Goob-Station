using Robust.Shared.GameStates;
using System;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.HoloCigar;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HoloCigarComponent : Component
{
    [ViewVariables]
    public bool Lit;
}

[Serializable, NetSerializable]
public sealed class HoloCigarComponentState: ComponentState
{
    public bool Lit;
    public HoloCigarComponentState(bool lit)
    {
        Lit = lit;
    }
}
