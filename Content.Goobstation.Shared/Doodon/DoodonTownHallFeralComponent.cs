using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Doodons;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DoodonTownHallFeralComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Feral = false;
}
