using Robust.Shared.GameStates;

namespace Content.Shared._Funkystation.MalfAI.Components;

/// <summary>
/// Marker component indicating a Malf AI has completed doomsday.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MalfAiDoomsdayCompletedComponent : Component;
