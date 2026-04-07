using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Bloodsuckers.Components;

/// <summary>
/// Present on a bloodsucker while a Feed session is active.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BloodsuckerFeedingComponent : Component
{
    /// <summary>
    /// Network-safe reference to the current feed target.
    /// </summary>
    [DataField, AutoNetworkedField]
    public NetEntity NetTarget = NetEntity.Invalid;
}
