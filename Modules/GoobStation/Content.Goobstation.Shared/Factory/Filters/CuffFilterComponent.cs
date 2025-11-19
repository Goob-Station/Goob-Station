using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Factory.Filters;

/// <summary>
/// Filter that requires a cuffable entity, and allows it if <c>IsCuffed() == ItemToggleComponent.Activated</c>.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CuffFilterComponent : Component;
