using Content.Goobstation.Shared.StationRadio.Components;
using Content.Goobstation.Shared.StationRadio.Events;
using Content.Shared.Interaction;
using Content.Shared.Power;
using Content.Shared.Power.EntitySystems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Shared.StationRadio.Systems;

public sealed class StationRadioReceiverSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StationRadioReceiverComponent, StationRadioMediaPlayedEvent>(OnMediaPlayed);
        SubscribeLocalEvent<StationRadioReceiverComponent, StationRadioMediaStoppedEvent>(OnMediaStopped);
        SubscribeLocalEvent<StationRadioReceiverComponent, ActivateInWorldEvent>(OnRadioToggle);
        SubscribeLocalEvent<StationRadioReceiverComponent, PowerChangedEvent>(OnPowerChanged);
    }

    private void OnPowerChanged(EntityUid uid, StationRadioReceiverComponent comp, PowerChangedEvent args)
    {
        if(comp.SoundEntity != null && args.Powered)
            _audio.SetGain(comp.SoundEntity, comp.Active ? 1f : 0f);
        else if(comp.SoundEntity != null)
            _audio.SetGain(comp.SoundEntity, 0);
    }

    private void OnRadioToggle(EntityUid uid, StationRadioReceiverComponent comp, ActivateInWorldEvent args)
    {
        comp.Active = !comp.Active;
        if (comp.SoundEntity != null && _power.IsPowered(uid))
            _audio.SetGain(comp.SoundEntity, comp.Active ? 1f : 0f);
    }

    private void OnMediaPlayed(EntityUid uid, StationRadioReceiverComponent comp, StationRadioMediaPlayedEvent args)
    {
        var gain = comp.Active ? 3f : 0f;
        var audio = _audio.PlayPredicted(args.MediaPlayed, uid, uid, AudioParams.Default.WithVolume(3f).WithMaxDistance(4.5f));
        if (audio != null && _power.IsPowered(uid))
        {
            comp.SoundEntity = audio.Value.Entity;
            _audio.SetGain(comp.SoundEntity, gain);
        }
        else if(audio != null && !_power.IsPowered(uid))
        {
            comp.SoundEntity = audio.Value.Entity;
            _audio.SetGain(comp.SoundEntity, 0);
        }
    }

    private void OnMediaStopped(EntityUid uid, StationRadioReceiverComponent comp, StationRadioMediaStoppedEvent args)
    {
        if (comp.SoundEntity != null)
            comp.SoundEntity = _audio.Stop(comp.SoundEntity);
    }
}
