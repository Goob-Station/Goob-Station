using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Heatlamp;

/// <summary>
///     Component that holds heatlamp state.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HeatlampComponent : Component;
