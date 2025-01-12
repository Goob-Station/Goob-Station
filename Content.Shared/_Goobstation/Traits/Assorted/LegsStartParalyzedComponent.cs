using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Traits.Assorted;

/// <summary>
/// Iterate through all the legs on the entity and make them paralyzed.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(LegsStartParalyzedSystem))]
public sealed partial class LegsStartParalyzedComponent : Component
{
}
