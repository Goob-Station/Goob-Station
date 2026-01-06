using Robust.Shared.GameStates;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Shared._CorvaxGoob.GhostBar;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GhostBarPlayerComponent : Component
{
    [ViewVariables]
    public ICommonSession? PlayerSession { get; set; }

    [DataField]
    public EntProtoId OpenGhostRolesListAction = "ActionOpenGhostRoles";

    [DataField, AutoNetworkedField]
    public EntityUid? OpenGhostRolesListActionEntity;
}
