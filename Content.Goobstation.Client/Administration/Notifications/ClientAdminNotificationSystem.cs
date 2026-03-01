using Content.Goobstation.Common.Administration.Notifications;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.Administration.Notifications;
using Robust.Client.Player;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Network;

namespace Content.Goobstation.Client.Administration.Notifications;

public sealed class ClientAdminNotificationsSystem : SharedAdminNotificationSystem
{
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private float _volume;

    public override void Initialize()
    {
        SubscribeNetworkEvent<AdminNotificationEvent>(OnAdminNotification);

        Subs.CVar(_config, GoobCVars.AdminNotificationVolume, v => _volume = SharedAudioSystem.GainToVolume(v), true);
    }

    public void OnAdminNotification(AdminNotificationEvent ev)
    {
        _audio.PlayGlobal(ev.Sound, _player.LocalSession!, new AudioParams().WithVolume(_volume));
    }
}
