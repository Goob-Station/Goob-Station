using Content.Goobstation.Shared.StationRadio.Components;
using Content.Shared.DeviceNetwork;
using Content.Shared.DeviceNetwork.Events;
using Content.Shared.DeviceNetwork.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Shared.StationRadio.Systems;

public sealed class StationRadioSystem : EntitySystem
{
    public const string PlayAudioCommand = "station_radio_play_audio";
    public const string StopAudioCommand = "station_radio_stop_audio";
    public const string SetAudioStateCommand = "station_radio_set_audio_state";

    public const string AudioPathData = "station_radio_data_audio_path";
    public const string AudioPlaybackData = "station_radio_data_audio_playback";
    public const string AudioStateData = "station_radio_data_audio_state";

    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDeviceNetworkSystem _device = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StationRadioServerComponent, DeviceNetworkPacketEvent>(OnServerRelay);
        SubscribeLocalEvent<StationRadioReceiverComponent, DeviceNetworkPacketEvent>(OnReceive);
    }

    private void OnServerRelay(Entity<StationRadioServerComponent> ent, ref DeviceNetworkPacketEvent args)
    {
        _device.QueuePacket(ent.Owner, null, args.Data);
    }

    private void OnReceive(Entity<StationRadioReceiverComponent> ent, ref DeviceNetworkPacketEvent args)
    {
        if (!args.Data.TryGetValue(DeviceNetworkConstants.Command, out string? command))
            return;

        switch (command)
        {
            case PlayAudioCommand:
                if (args.Data.TryGetValue(AudioPathData, out SoundSpecifier? sound))
                    PlayAudio(ent, sound);
                break;
            case StopAudioCommand:
                ent.Comp.SoundEntity = _audio.Stop(ent.Comp.SoundEntity);
                break;
            case SetAudioStateCommand:
                if (args.Data.TryGetValue(AudioStateData, out AudioState state))
                    _audio.SetState(ent.Comp.SoundEntity, state);
                break;
        }
    }

    private void PlayAudio(Entity<StationRadioReceiverComponent> ent, SoundSpecifier? sound)
    {
        var audio = _audio.PlayPvs(sound,
            ent.Owner,
            AudioParams.Default.WithVolume(3f).WithMaxDistance(4.5f));
        if (audio != null)
            ent.Comp.SoundEntity = audio.Value.Entity;
    }
}
