using Content.Goobstation.Shared.StationRadio.Components;
using Content.Goobstation.Shared.StationRadio.Events;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Shared.StationRadio.Systems;

public sealed class StationRadioReceiverSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StationRadioReceiverComponent, StationRadioMediaPlayedEvent>(OnMediaPlayed);
        SubscribeLocalEvent<StationRadioReceiverComponent, StationRadioMediaStoppedEvent>(OnMediaStopped);
    }

    private void OnMediaPlayed(EntityUid uid, StationRadioReceiverComponent comp, StationRadioMediaPlayedEvent args)
    {
        var audio = _audio.PlayPredicted(args.MediaPlayed, uid, uid);
        if (audio != null)
            comp.SoundEntity = audio.Value.Entity;
    }

    private void OnMediaStopped(EntityUid uid, StationRadioReceiverComponent comp, StationRadioMediaStoppedEvent args)
    {
        if(comp.SoundEntity != null)
            _audio.Stop(comp.SoundEntity);
    }
}
