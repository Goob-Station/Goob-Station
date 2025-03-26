using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class VoidCurseComponent : Component
{
    [DataField, AutoNetworkedField] public float Lifetime = 20f;
    [DataField] public float MaxLifetime = 60f;
    [AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)] public bool Drain = false;

    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField] public float Stacks = 1f;
    [DataField, AutoNetworkedField] public float MaxStacks = 5f;

    public float Timer = 1f;
}
