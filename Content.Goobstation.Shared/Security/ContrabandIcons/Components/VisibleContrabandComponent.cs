using Content.Shared._Goobstation.Security.ContrabandIcons.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Security.ContrabandIcons.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class VisibleContrabandComponent : Component
{
    /// <summary>
    ///     The icon that should be displayed based on the criminal status of the entity.
    /// </summary>
    [DataField, AutoNetworkedField] 
    public ProtoId<ContrabandIconPrototype> StatusIcon = "ContrabandIconNone";
}
