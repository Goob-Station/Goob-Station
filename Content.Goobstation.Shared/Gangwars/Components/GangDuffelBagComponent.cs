using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Gangwars.Components;

/// <summary>
/// Handles the gang duffel bag.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(raiseAfterAutoHandleState: true)]
public sealed partial class GangDuffelBagComponent : Component
{
    public static readonly EntProtoId TrappedStatusEffect = "GangDuffelBagTrappedStatusEffect";

    [DataField, AutoNetworkedField]
    public GangDuffelBagState State = GangDuffelBagState.Trapped;

    [DataField]
    public TimeSpan UntrapTime = TimeSpan.FromSeconds(6);

    [DataField]
    public TimeSpan SlowDuration = TimeSpan.FromSeconds(15);
}

[Serializable, NetSerializable]
public enum GangDuffelBagState : byte
{
    Trapped,
    Closed,
    Open,
}

