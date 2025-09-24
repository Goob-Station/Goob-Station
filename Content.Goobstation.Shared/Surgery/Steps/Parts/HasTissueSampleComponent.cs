using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Surgery.Steps.Parts;

/// <summary>
/// Added to xeno limbs and removed to prevent getting infinite samples from them.
/// Also used to allow adding xeno organ slots to humanoid limbs.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HasTissueSampleComponent : Component;
