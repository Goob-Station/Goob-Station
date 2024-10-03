using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Interaction.Components;

[RegisterComponent,NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class StopOnLOSComponent : Component
{
    [DataField]
    [AutoNetworkedField]
    public bool canMove = false;
}
