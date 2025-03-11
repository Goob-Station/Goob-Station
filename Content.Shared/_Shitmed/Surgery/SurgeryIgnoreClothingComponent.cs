using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Medical.Surgery;

/// <summary>
///     Allows the entity to do surgery without having to remove clothing.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryIgnoreClothingComponent : Component { }
