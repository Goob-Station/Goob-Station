// SPDX-License-Identifier: MIT

using Robust.Shared.Audio.Systems;
using Robust.Shared.Serialization; // Pirate

namespace Content.Shared.Audio.Jukebox;

public abstract class SharedJukeboxSystem : EntitySystem
{
    [Dependency] protected readonly SharedAudioSystem Audio = default!;
}

// Pirate: Shuffle & Repeat
[Serializable, NetSerializable]
public sealed class JukeboxInterfaceState(JukeboxPlaybackMode playbackMode) : BoundUserInterfaceState
{
    public JukeboxPlaybackMode PlaybackMode { get; set; } = playbackMode;
}
// End Pirate: Shuffle & Repeat
