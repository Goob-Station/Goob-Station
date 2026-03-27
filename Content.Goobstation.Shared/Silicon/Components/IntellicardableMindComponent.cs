using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Silicon.Components;

/// <summary>
/// Declares that this entity's MindContainerComponent can be transferred to/from via an intellicard.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class IntellicardableMindComponent : Component;
