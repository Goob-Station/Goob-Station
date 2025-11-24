using System.Numerics;
using Robust.Shared.Audio.Components;
using Robust.Shared.GameStates;

namespace Content.Trauma.Shared.AudioMuffle;

/// <summary>
/// Added to obstacles that block sound, used in audio muffle calculations
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true, true)]
public sealed partial class SoundBlockerComponent : Component
{
    /// <summary>
    /// Muffle strength.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float SoundBlockPercent = 0.7f;

    /// <summary>
    /// Is blocker currently active.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Active = true;

    /// <summary>
    /// Grid indices of the blocker, if it is on grid and pathfinding behavior is enabled
    /// </summary>
    [ViewVariables]
    public Vector2i? Indices;

    /// <summary>
    /// Cached blocker cost calculated from <see cref="SoundBlockPercent"/>
    /// </summary>
    [ViewVariables]
    public float? CachedBlockerCost;
}
