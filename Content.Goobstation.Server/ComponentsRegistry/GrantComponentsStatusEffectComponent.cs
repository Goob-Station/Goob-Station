using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.ComponentsRegistry;

[RegisterComponent, NetworkedComponent]
public sealed partial class GrantComponentsStatusEffectComponent : Component
{
    [DataField(required: true)]
    public ComponentRegistry Components { get; private set; } = new();
}
