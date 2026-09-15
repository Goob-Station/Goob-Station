using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Bloodsuckers.Components;

/// <summary>
/// Added to a bloodsucker while Masquerade is active.
/// Makes health analyzers and examine show normal human vitals.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BloodsuckerMasqueradingComponent : Component;
