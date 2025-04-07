using Content.Shared.Actions;
using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Body;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class CursedHeartComponent : Component
{
    [AutoNetworkedField]
    public EntityUid? PumpActionEntity;

    public TimeSpan LastPump = TimeSpan.Zero;

    [DataField]
    public float MaxDelay = 5f;
}

public sealed partial class PumpHeartActionEvent : InstantActionEvent;
