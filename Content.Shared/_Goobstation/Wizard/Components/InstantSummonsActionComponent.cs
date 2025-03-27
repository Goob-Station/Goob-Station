using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Wizard.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class InstantSummonsActionComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public EntityUid? Entity;
}
