using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Oskarrr.Yautja.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class YautjaObservationPadComponent : Component
{
    [DataField]
    public EntProtoId ProjectionPrototype = "GoobYautjaObservationProjection";

    /// <summary>Action granted while projected, for returning to the body.</summary>
    [DataField]
    public EntProtoId ReturnActionPrototype = "ActionYautjaObservationReturn";

    /// <summary>Cooldown before return is available after projecting.</summary>
    [DataField]
    public TimeSpan ReturnDelay = TimeSpan.FromSeconds(3);
}

/// <summary>Marker on the spawned projection entity; stores the body to return to.</summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class YautjaObservationProjectionComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid OriginalBody;
}

public sealed partial class YautjaObservationProjectionEvent : InstantActionEvent;

public sealed partial class YautjaObservationReturnEvent : InstantActionEvent;
