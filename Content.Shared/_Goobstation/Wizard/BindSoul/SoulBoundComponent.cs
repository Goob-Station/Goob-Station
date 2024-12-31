using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Wizard.BindSoul;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SoulBoundComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public EntityUid? Item;

    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public EntityUid? MapId;

    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public string Name = string.Empty;

    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public int ResurrectionsCount;
}
