using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Factory;

/// <summary>
/// Component added to machines with <see cref="AutomationSlotsComponent"/> to enable their ports for linking.
/// They can then be automated with things like a <see cref="RoboticArmComponent"/>.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AutomatedComponent : Component;
