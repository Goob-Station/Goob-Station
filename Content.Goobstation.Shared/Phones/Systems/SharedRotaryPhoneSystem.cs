using Content.Goobstation.Shared.Phones.Components;
using Content.Goobstation.Shared.Phones.Events;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Examine;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Storage.Components;
using Content.Shared.Verbs;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.Phones.Systems;

public sealed class SharedRotaryPhoneSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedJointSystem _jointSystem = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<RotaryPhoneComponent, PhoneRingEvent>(OnRing);
        SubscribeLocalEvent<RotaryPhoneComponent, PhoneHungUpEvent>(OnGotHungUp);
        SubscribeLocalEvent<RotaryPhoneComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<RotaryPhoneComponent, BoundUIClosedEvent>(OnUiClosed);
        SubscribeLocalEvent<RotaryPhoneComponent, EntGotRemovedFromContainerMessage>(OnPickup);
        SubscribeLocalEvent<RotaryPhoneComponent, EntGotInsertedIntoContainerMessage>(OnHangUp);
        SubscribeLocalEvent<RotaryPhoneComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
        SubscribeLocalEvent<RotaryPhoneComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<RotaryPhoneHolderComponent, ExaminedEvent>(OnExamineHolder);
        SubscribeLocalEvent<RotaryPhoneHolderComponent, EntInsertedIntoContainerMessage>(OnPhoneInsertHolder);
        SubscribeLocalEvent<RotaryPhoneHolderComponent, ItemSlotInsertAttemptEvent>(OnInsertAttempt);
    }
    private void OnMapInit(EntityUid uid, RotaryPhoneComponent comp, MapInitEvent args)
    {
        if(comp.PhoneNumber == null)
            comp.PhoneNumber = _random.Next(11111,99999);
    }

    private void OnExamine(EntityUid uid, RotaryPhoneComponent comp, ExaminedEvent args)
    {
        if(comp.PhoneNumber != null)
            args.PushMarkup(Loc.GetString("phone-number-description", ("number", comp.PhoneNumber)));
    }

    private void OnExamineHolder(EntityUid uid, RotaryPhoneHolderComponent comp, ExaminedEvent args)
    {
        if (comp.PhoneNumber != null)
            args.PushMarkup(Loc.GetString("phone-number-description", ("number", comp.PhoneNumber)));
    }


    private void OnGetVerbs(EntityUid uid, RotaryPhoneComponent comp, GetVerbsEvent<AlternativeVerb> args)
    {
        if (args.Hands == null || args.Using == null || !args.CanAccess || !args.CanInteract)
            return;

        AlternativeVerb verb = new()
        {
            Text = Loc.GetString("phone-speakerphone"),
            Message = Loc.GetString("phone-speakerphone-message"),
            Act = () =>
            {
                comp.SpeakerPhone = !comp.SpeakerPhone;
                Dirty(uid, comp);

                var state = Loc.GetString(comp.SpeakerPhone ? "handheld-radio-component-on-state" : "handheld-radio-component-off-state");
                var message = Loc.GetString("phone-speakerphone-onoff", ("status", state));
                _popupSystem.PopupPredicted(message, uid, args.User);
            }
        };
        args.Verbs.Add(verb);
    }

    private void OnPhoneInsertHolder(EntityUid uid, RotaryPhoneHolderComponent comp, EntInsertedIntoContainerMessage args)
    {
        if (TryComp<RotaryPhoneComponent>(args.Entity, out var phone))
        {
            comp.PhoneNumber = phone.PhoneNumber;
        }
    }

    private void OnInsertAttempt(EntityUid uid, RotaryPhoneHolderComponent comp, ref ItemSlotInsertAttemptEvent args)
    {
        if(!TryComp<RotaryPhoneComponent>(args.Item, out var phone))
            return;

        if(phone.PhoneNumber != comp.PhoneNumber)
            args.Cancelled = true;
    }


    private void OnUiClosed(EntityUid uid, RotaryPhoneComponent comp, BoundUIClosedEvent args)
    {
        comp.DialedNumber = null;
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
        comp.ConnectedPlayer = null;

        if (!TryComp<RotaryPhoneHolderComponent>(args.Container.Owner, out var _))
            return;

        comp.Engaged = true;

        if (_net.IsServer && !Deleted(uid) && !Terminating(uid))
        {
            var visuals = EnsureComp<JointVisualsComponent>(uid);
            visuals.Sprite = comp.RopeSprite;
            visuals.Target = GetNetEntity(args.Container.Owner);
            Dirty(uid, visuals);
        }

        if(comp.ConnectedPhone == null || !TryComp<RotaryPhoneComponent>(comp.ConnectedPhone, out var otherPhone) )
            return;

        ConnectPhones(comp, otherPhone, uid);
    }

    private void OnHangUp(EntityUid uid, RotaryPhoneComponent comp, EntGotInsertedIntoContainerMessage args)
    {
        if(TryComp<ActorComponent>(args.Container.Owner, out var _))
            comp.ConnectedPlayer = args.Container.Owner;

        if (!TryComp<RotaryPhoneHolderComponent>(args.Container.Owner, out var _))
            return;

        if (_net.IsServer)
        {
            RemComp<JointVisualsComponent>(uid);
            Dirty(uid, comp);
        }

        DisconnectPhones(comp);

    }
    private void OnGotHungUp(EntityUid uid, RotaryPhoneComponent comp, PhoneHungUpEvent args)
    {
        if(!comp.Connected)
            return;

        var audio = _audio.PlayPvs(comp.HandUpSoundLocal, uid);
        if (audio != null)
            comp.SoundEntity = audio.Value.Entity;

        comp.ConnectedPhone = null;
        comp.Connected = false;
    }

    //Helper Functions

    private void ConnectPhones(RotaryPhoneComponent thisPhone, RotaryPhoneComponent otherPhone, EntityUid uid)
    {
        thisPhone.Connected = true;
        otherPhone.Connected = true;
        otherPhone.ConnectedPhone = uid;

        if(otherPhone.SoundEntity != null)
            otherPhone.SoundEntity = _audio.Stop(otherPhone.SoundEntity);

        if (thisPhone.SoundEntity != null)
            thisPhone.SoundEntity = _audio.Stop(thisPhone.SoundEntity);
    }

    private void DisconnectPhones(RotaryPhoneComponent thisPhone)
    {
        if (thisPhone.ConnectedPhone != null)
        {
            RaiseLocalEvent(thisPhone.ConnectedPhone.Value, new PhoneHungUpEvent());

            if (!thisPhone.Connected && TryComp<RotaryPhoneComponent>(thisPhone.ConnectedPhone, out var otherPhone))
            {
                if (otherPhone.SoundEntity != null)
                    otherPhone.SoundEntity = _audio.Stop(otherPhone.SoundEntity);

                otherPhone.ConnectedPhone = null;
                otherPhone.Engaged = false;
            }
        }

        if (thisPhone.SoundEntity != null)
            thisPhone.SoundEntity = _audio.Stop(thisPhone.SoundEntity);

        thisPhone.ConnectedPhone = null;
        thisPhone.Engaged = false;
        thisPhone.Connected = false;
    }

}
