using Content.Goobstation.Common.Administration.Notifications;
using Content.Goobstation.Shared.Administration.Notifications;
using Content.Server.Administration.Managers;
using Robust.Shared.Audio;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Administration.Notifications;

public sealed class ServerAdminNotificationsSystem : SharedAdminNotificationSystem
{
    [Dependency] private readonly IAdminManager _admin = default!;

    /// <inheritdoc/>
    public override void PlayNotification(SoundSpecifier? path)
    {
        foreach (var admin in _admin.ActiveAdmins)
        {
            PlayNotification(path, admin);
        }
    }

    /// <inheritdoc/>
    public override void PlayNotification(SoundSpecifier? path, ICommonSession session)
    {
        if (path == null)
            return;

        RaiseNetworkEvent(new AdminNotificationEvent(path), session);
    }
}
