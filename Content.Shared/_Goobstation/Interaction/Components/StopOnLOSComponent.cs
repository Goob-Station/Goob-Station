using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Interaction.Components;

[RegisterComponent,NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class StopOnLOSComponent : Component
{
    [AutoNetworkedField]
    public bool CanMove = false;

    [DataField]
    [AutoNetworkedField]
    public float SightRange = 12f;

    [DataField]
    [AutoNetworkedField]
    public float SightAngle = 120f;
}
