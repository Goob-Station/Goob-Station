using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Factory.Filters;

/// <summary>
/// Filter that requires an anchorable entity, and allows it if <c>Anchored == ItemToggleComponent.Activated</c>.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AnchorFilterComponent : Component;
