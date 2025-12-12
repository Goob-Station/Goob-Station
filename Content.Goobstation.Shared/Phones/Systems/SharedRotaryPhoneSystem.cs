using Content.Goobstation.Shared.Phones.Components;
using Content.Goobstation.Shared.Phones.Events;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.Phones.Systems;

public sealed class SharedRotaryPhoneSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<RotaryPhoneComponent, PhoneRingEvent>(OnRing);
        SubscribeLocalEvent<RotaryPhoneComponent, PhoneHungUpEvent>(OnGotHungUp);
        SubscribeLocalEvent<RotaryPhoneComponent, EntGotRemovedFromContainerMessage>(OnPickup);
        SubscribeLocalEvent<RotaryPhoneComponent, EntGotInsertedIntoContainerMessage>(OnHangUp);
    }

    private void OnRing(EntityUid uid, RotaryPhoneComponent comp, PhoneRingEvent args)
    {
        var audio = _audio.PlayPvs(comp.RingSound, uid, AudioParams.Default.WithLoop(true));

        comp.ConnectedPhone = args.phone;

        if(audio != null)
            comp.SoundEntity = audio.Value.Entity;
    }

    private void OnPickup(EntityUid uid, RotaryPhoneComponent comp, EntGotRemovedFromContainerMessage args)
    {

        comp.Engaged = true;

        if (!TryComp<RotaryPhoneComponent>(comp.ConnectedPhone, out var otherPhone) || comp.ConnectedPhone == null)
            return;

        comp.Connected = true;
        otherPhone.Connected = true;
        otherPhone.ConnectedPhone = uid;

        if(otherPhone.SoundEntity != null)
            otherPhone.SoundEntity = _audio.Stop(otherPhone.SoundEntity);

        if (comp.SoundEntity != null)
            comp.SoundEntity = _audio.Stop(comp.SoundEntity);
    }

    private void OnHangUp(EntityUid uid, RotaryPhoneComponent comp, EntGotInsertedIntoContainerMessage args)
    {
        if(!TryComp<RotaryPhoneHolderComponent>(args.Container.Owner, out var _))
            return;

        if (comp.ConnectedPhone != null)
        {
            RaiseLocalEvent(comp.ConnectedPhone.Value, new PhoneHungUpEvent());
        }

        if (comp.SoundEntity != null)
            comp.SoundEntity = _audio.Stop(comp.SoundEntity);

        comp.ConnectedPhone = null;
        comp.Engaged = false;
        comp.Connected = false;
    }
    private void OnGotHungUp(EntityUid uid, RotaryPhoneComponent comp, PhoneHungUpEvent args)
    {
        var audio = _audio.PlayPvs(comp.HandUpSoundLocal, uid);
        if (audio != null)
            comp.SoundEntity = audio.Value.Entity;

        comp.ConnectedPhone = null;
        comp.Engaged = false;
        comp.Connected = false;
    }
}
