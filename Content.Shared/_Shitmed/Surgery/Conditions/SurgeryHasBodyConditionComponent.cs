using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Medical.Surgery.Conditions;

/// <summary>
/// Requires that this part is attached to a body for the surgery to be done.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryHasBodyConditionComponent : Component;
