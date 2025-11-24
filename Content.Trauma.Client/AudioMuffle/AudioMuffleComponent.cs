using Content.Trauma.Shared.AudioMuffle;

namespace Content.Trauma.Client.AudioMuffle;

/// <summary>
/// Component added to all muffleable audio clientside, stores audio muffle data.
/// </summary>
[RegisterComponent]
public sealed partial class AudioMuffleComponent : Component
{
    /// <summary>
    /// Volume before muffle is applied.
    /// </summary>
    [ViewVariables]
    public float? OriginalVolume;

    /// <summary>
    /// Set of blockers between player and audio entity.
    /// Used in raycast calculations.
    /// </summary>
    [ViewVariables]
    public HashSet<Entity<SoundBlockerComponent>> RayBlockers = new();

    /// <summary>
    /// Grid indices of audio entity.
    /// If null, muffle is controlled by raycast behavior, otherwise by pathfinding behavior.
    /// </summary>
    [ViewVariables]
    public Vector2i? Indices;
}
