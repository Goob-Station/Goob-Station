using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Wizard.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SwapSpellComponent : Component
{
    [DataField]
    public bool AllowSecondaryTarget = true;

    [DataField, AutoNetworkedField]
    public EntityUid? SecondaryTarget;
}
