using Content.Goobstation.Common.Administration.Notifications;
using Content.Shared.Administration.Managers;
using Robust.Shared.Audio;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Goobstation.Shared.Administration.Notifications;

public abstract class SharedAdminNotificationSystem : EntitySystem
{
    /// <summary>
    ///     Play a notification for all active admins, does nothing if called from the client
    /// </summary>
    public virtual void PlayNotification(SoundSpecifier? path) {}

    /// <summary>
    ///     Play a notification for a specific client, does nothing if called from the client
    /// </summary>
    public virtual void PlayNotification(SoundSpecifier? path, ICommonSession session) {}
}
