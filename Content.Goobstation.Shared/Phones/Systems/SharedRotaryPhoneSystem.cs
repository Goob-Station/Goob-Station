using Content.Goobstation.Shared.Phones.Components;
using Content.Goobstation.Shared.Phones.Events;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Examine;
using Content.Shared.Popups;
using Content.Shared.Stacks;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.Phones.Systems;

public sealed class SharedRotaryPhoneSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<RotaryPhoneComponent, PhoneRingEvent>(OnRing);
        SubscribeLocalEvent<RotaryPhoneComponent, PhoneHungUpEvent>(OnGotHungUp);
        SubscribeLocalEvent<RotaryPhoneComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<RotaryPhoneHolderComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<RotaryPhoneComponent, BoundUIClosedEvent>(OnUiClosed);
        SubscribeLocalEvent<RotaryPhoneComponent, EntGotRemovedFromContainerMessage>(OnPickup);
        SubscribeLocalEvent<RotaryPhoneComponent, EntGotInsertedIntoContainerMessage>(OnHangUp);
    }

    private void OnUiClosed(EntityUid uid, RotaryPhoneComponent comp, BoundUIClosedEvent args)
    {
        comp.DialedNumber = null;
    }

    private void OnMapInit(EntityUid uid, RotaryPhoneComponent comp, MapInitEvent args)
    {
        comp.PhoneNumber = _random.Next(111,999);
    }

    private void OnExamine(EntityUid uid, RotaryPhoneHolderComponent comp, ExaminedEvent args)
    {
        if(!_itemSlots.TryGetSlot(uid, "phone", out var phoneslot))
            return;

        RotaryPhoneComponent? stack = null;
        if (phoneslot.Item == null || !TryComp(phoneslot.Item.Value, out stack) || stack.PhoneNumber == null)
            return;


        args.PushMarkup(Loc.GetString("phone-number-description", ("number", stack.PhoneNumber)));
    }

    private void OnRing(EntityUid uid, RotaryPhoneComponent comp, PhoneRingEvent args)
    {
        var audio = _audio.PlayPvs(comp.RingSound, uid, AudioParams.Default.WithLoop(true));

        _popupSystem.PopupPredicted(Loc.GetString("phone-popup-ring", ("location", args.otherPhoneComponent.Name ?? "Unknown")), uid, args.phone, PopupType.Medium);

        comp.ConnectedPhone = args.phone;

        if(audio != null)
            comp.SoundEntity = audio.Value.Entity;
    }

    private void OnPickup(EntityUid uid, RotaryPhoneComponent comp, EntGotRemovedFromContainerMessage args)
    {

        if (!TryComp<RotaryPhoneComponent>(comp.ConnectedPhone, out var otherPhone) || comp.ConnectedPhone == null || !TryComp<RotaryPhoneHolderComponent>(args.Container.Owner, out var _))
            return;

        comp.Engaged = true;

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

            if (!comp.Connected && TryComp<RotaryPhoneComponent>(comp.ConnectedPhone, out var otherPhone))
            {
                if (otherPhone.SoundEntity != null)
                    otherPhone.SoundEntity = _audio.Stop(otherPhone.SoundEntity);

                otherPhone.ConnectedPhone = null;
                otherPhone.Engaged = false;
            }
        }

        if (comp.SoundEntity != null)
            comp.SoundEntity = _audio.Stop(comp.SoundEntity);

        comp.ConnectedPhone = null;
        comp.Engaged = false;
        comp.Connected = false;
    }
    private void OnGotHungUp(EntityUid uid, RotaryPhoneComponent comp, PhoneHungUpEvent args)
    {
        if(!comp.Connected)
            return;

        var audio = _audio.PlayPvs(comp.HandUpSoundLocal, uid);
        if (audio != null)
            comp.SoundEntity = audio.Value.Entity;

        comp.ConnectedPhone = null;
        comp.Engaged = false;
        comp.Connected = false;
    }
}
