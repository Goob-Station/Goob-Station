using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Medical.Surgery.Steps;

/// <summary>
/// A surgery step that removes every surgically-treatable trauma on the target part.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryTraumaExtractStepComponent : Component;
