using Robust.Shared.GameStates;

namespace Content.Shared._White.Xenomorphs.Queen;

/// <summary>
/// Marker: this xenomorph can use consoles/machines (complex interactions). Still cannot pick up normal items.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class XenomorphMachineUserComponent : Component;
