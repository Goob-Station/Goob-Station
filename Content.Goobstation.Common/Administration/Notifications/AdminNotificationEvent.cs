using Robust.Shared.Audio;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Common.Administration.Notifications;

/// <summary>
///     Plays a sound on the client if they have the admin notification cvar enabled.
/// </summary>
[Serializable, NetSerializable]
public sealed class AdminNotificationEvent(SoundSpecifier sound) : EntityEventArgs
{
    public SoundSpecifier Sound { get; } = sound;
}
