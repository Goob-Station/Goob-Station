using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared._ES.Interaction.ForcedFacing;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(ESForcedFacingSystem))]
public sealed partial class ESForcedFacingComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<EntityUid> Targets = new();

    [ViewVariables]
    public EntityUid? PrimaryTarget => Targets.FirstOrNull();
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(ESForcedFacingSystem))]
public sealed partial class ESForcedFacingTargetComponent : Component
{
    [DataField, AutoNetworkedField]
    public HashSet<EntityUid> Facing = new();
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ESForcedFacingUiComponent : Component;

[ByRefEvent]
public record struct ESRefreshNoRotateOnMoveEvent(bool Enabled = false);

[ByRefEvent]
public record struct ESRefreshNoRotateOnInteractEvent(bool Enabled = false);

[ByRefEvent]
public record struct ESRefreshForcedFacingEvent()
{
    public HashSet<EntityUid> Targets = new();
}
