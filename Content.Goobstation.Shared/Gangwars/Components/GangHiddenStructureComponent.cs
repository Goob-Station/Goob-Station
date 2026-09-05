using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Gangwars.Components;

/// <summary>
/// Lets a gang structure (e.g. the gang locker) hide itself under the floor after a period of
/// inactivity and reveal again when a same-gang member interacts with it.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(raiseAfterAutoHandleState: true)]
public sealed partial class GangHiddenStructureComponent : Component
{
    [DataField]
    public float HideDelay = 20f;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    public TimeSpan LastInteractTime;

    [DataField, AutoNetworkedField]
    public bool IsHidden;

    [DataField]
    public LocId RevealPopup = "gang-structure-revealed";
}
