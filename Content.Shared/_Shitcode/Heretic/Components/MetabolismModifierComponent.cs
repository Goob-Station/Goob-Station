using Robust.Shared.GameStates;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed class MetabolismModifierComponent : Component
{
    [DataField(required: true)]
    public float Modifier;
}
