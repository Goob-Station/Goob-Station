using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// Makes the entity's sprite fade out in darkness and back in under light.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BoogymanShadowComponent : Component;
