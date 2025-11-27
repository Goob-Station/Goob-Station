using Content.Server._Funkystation.Objectives.Systems;

namespace Content.Server._Funkystation.Objectives.Components;

/// <summary>
/// Component for handling protect target selection, prioritizing traitors and traitor targets.
/// </summary>
[RegisterComponent, Access(typeof(MalfAiPickProtectTargetSystem))]
public sealed partial class MalfAiPickProtectTargetComponent : Component;
