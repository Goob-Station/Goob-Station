using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Common.Traits;

/// <summary>
/// Iterate through all the legs on the entity and make them paralyzed.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class LegsStartParalyzedComponent : Component;
