using Content.Goobstation.Shared.Security.ContrabandIcons.Prototypes;
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

    [DataField] 
    public HashSet<EntityUid> VisibleItems = new();
    
    [DataField]
    public TimeSpan VisibleTimeout = TimeSpan.FromSeconds(5f);

    [DataField] 
    public TimeSpan? FirstItemTime;
}

public enum ContrabandStatus : byte
{
    None,
    Contraband
}
