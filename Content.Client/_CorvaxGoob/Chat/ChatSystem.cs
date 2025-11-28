using Content.Shared._CorvaxGoob.CCCVars;
using Content.Shared._CorvaxGoob.Chat;
using Robust.Client.Player;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Player;

namespace Content.Client._CorvaxGoob.Chat;
public sealed class ChatSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private float _announcementsVolume = 0.0f;

    public override void Initialize()
    {
        base.Initialize();

        _cfg.OnValueChanged(CCCVars.AnnouncementsSound, OnAnnouncementsVolumeChanged, true);
        SubscribeNetworkEvent<PlayGlobalSoundEvent>(OnPlayGlobalSound);
    }

    private void OnAnnouncementsVolumeChanged(float volume)
    {
        _announcementsVolume = volume;
    }

    private void OnPlayGlobalSound(PlayGlobalSoundEvent ev)
    {
        if (_playerManager.LocalSession is not null)
        {
            var audioParams =
                ev.AudioParams.HasValue
                ? ev.AudioParams.Value.WithVolume(SharedAudioSystem.GainToVolume(_announcementsVolume))
                : AudioParams.Default.WithVolume(SharedAudioSystem.GainToVolume(_announcementsVolume));

            _audio.PlayGlobal(_audio.ResolveSound(ev.SoundSpecifier), Filter.Local(), false, audioParams);

        }
    }
}
