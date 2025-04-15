using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Enchanting.Components;

/// <summary>
/// Added to a mind or mob to prevent it upgrading enchanted items when killed.
/// Gets added to both after a successful sacrafice.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SoullessComponent : Component;
