using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Execution;

/// <summary>
/// Used in any guns that shouldn't be able to be used for exucutions
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class GunExecutionBlacklistComponent : Component;
