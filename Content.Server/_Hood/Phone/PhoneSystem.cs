// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Telephone;
using Content.Shared._Hood.Phone;
using Content.Shared.GameTicking;
using Content.Shared.Telephone;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Server._Hood.Phone;

/// <summary>
/// Server authority for Hood smartphone identity, round-session SMS, and the adapter to upstream telephones.
/// Speech relay and telephone state transitions remain owned by <see cref="TelephoneSystem"/>.
/// </summary>
public sealed partial class PhoneSystem : SharedPhoneSystem
{
    [Dependency] private readonly TelephoneSystem _telephone = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PhoneComponent, BoundUIOpenedEvent>(OnUiOpened);
        SubscribeLocalEvent<PhoneComponent, EntInsertedIntoContainerMessage>(OnContainerModified);
        SubscribeLocalEvent<PhoneComponent, EntRemovedFromContainerMessage>(OnContainerModified);
        SubscribeLocalEvent<PhoneComponent, TelephoneStateChangeEvent>(OnTelephoneStateChanged);
        SubscribeLocalEvent<PhoneComponent, ComponentShutdown>(OnPhoneShutdown);
        SubscribeLocalEvent<SimCardComponent, ComponentShutdown>(OnSimShutdown);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);

        SubscribeLocalEvent<PhoneComponent, PhoneSendSmsMessage>(OnSendSms);
        SubscribeLocalEvent<PhoneComponent, PhoneDialMessage>(OnDial);
        SubscribeLocalEvent<PhoneComponent, PhoneAcceptCallMessage>(OnAcceptCall);
        SubscribeLocalEvent<PhoneComponent, PhoneRejectCallMessage>(OnRejectCall);
        SubscribeLocalEvent<PhoneComponent, PhoneHangupMessage>(OnHangup);
    }

    private void OnUiOpened(Entity<PhoneComponent> ent, ref BoundUIOpenedEvent args)
    {
        if (args.UiKey.Equals(PhoneUiKey.Key))
            UpdateUi(ent);
    }

    private void OnContainerModified(Entity<PhoneComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != PhoneComponent.SimSlotId)
            return;

        ClearCallDisposition(ent.Owner);
        UpdateUi(ent);
    }

    private void OnContainerModified(Entity<PhoneComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != PhoneComponent.SimSlotId)
            return;

        EndCallForLifecycle(ent);

        // ContainerSlot raises this event before it clears Item. Refresh on the next tick so the
        // server-derived snapshot cannot briefly report the ejected SIM as still installed.
        var phoneUid = ent.Owner;
        Timer.Spawn(0, () =>
        {
            if (!TerminatingOrDeleted(phoneUid) && TryComp<PhoneComponent>(phoneUid, out var phone))
                UpdateUi((phoneUid, phone));
        });
    }

    private void OnTelephoneStateChanged(Entity<PhoneComponent> ent, ref TelephoneStateChangeEvent args)
    {
        if (args.NewState is TelephoneState.Calling or TelephoneState.Ringing or TelephoneState.InCall)
            ClearCallDisposition(ent.Owner);
        else if (args.NewState == TelephoneState.EndingCall && GetCallDisposition(ent.Owner) == PhoneCallDisposition.None)
            SetCallDisposition(ent.Owner, PhoneCallDisposition.Ended);

        UpdateUi(ent);
    }

    private void OnPhoneShutdown(Entity<PhoneComponent> ent, ref ComponentShutdown args)
    {
        TerminateCallForLifecycle(ent);
        RemoveCallDisposition(ent.Owner);
    }

    private void OnSimShutdown(Entity<SimCardComponent> ent, ref ComponentShutdown args)
    {
        if (TryFindPhoneUsingSim(ent.Owner, out var phone))
            TerminateCallForLifecycle(phone);

        RemoveSmsSession(ent.Owner);
    }

    private void OnRoundRestart(RoundRestartCleanupEvent args)
    {
        ClearSmsSessions();
        ClearCallDispositions();
    }

    private void OnSendSms(Entity<PhoneComponent> ent, ref PhoneSendSmsMessage args)
    {
        var error = SendSms(ent, args.RecipientNumber, args.Content, args.RequestId, args.Actor);
        UpdateUi(ent, error);
    }

    private void OnDial(Entity<PhoneComponent> ent, ref PhoneDialMessage args)
    {
        UpdateUi(ent, Dial(ent, args.RecipientNumber, args.Actor));
    }

    private void OnAcceptCall(Entity<PhoneComponent> ent, ref PhoneAcceptCallMessage args)
    {
        UpdateUi(ent, AcceptCall(ent, args.Actor));
    }

    private void OnRejectCall(Entity<PhoneComponent> ent, ref PhoneRejectCallMessage args)
    {
        UpdateUi(ent, RejectCall(ent));
    }

    private void OnHangup(Entity<PhoneComponent> ent, ref PhoneHangupMessage args)
    {
        UpdateUi(ent, Hangup(ent));
    }

    /// <summary>
    /// Sends a fresh, server-derived snapshot. An error is intentionally transient and belongs to this response.
    /// </summary>
    public void UpdateUi(Entity<PhoneComponent> ent, PhoneOperationError error = PhoneOperationError.None)
    {
        if (TerminatingOrDeleted(ent.Owner) || !_userInterface.HasUi(ent.Owner, PhoneUiKey.Key))
            return;

        var hasSim = TryGetPhoneIdentity(ent, out var sim, out var phoneNumber);
        var conversations = hasSim
            ? BuildConversationSnapshot(sim.Owner)
            : new Dictionary<uint, List<PhoneSmsMessage>>();

        var callState = TelephoneState.Idle;
        uint? peerNumber = null;
        if (TryComp<TelephoneComponent>(ent.Owner, out var telephone))
        {
            callState = telephone.CurrentState;
            foreach (var linked in telephone.LinkedTelephones)
            {
                if (TryComp<PhoneComponent>(linked.Owner, out var peerPhone) &&
                    TryGetPhoneIdentity((linked.Owner, peerPhone), out _, out var number))
                {
                    peerNumber = number;
                }

                break;
            }
        }

        var state = new PhoneBoundUserInterfaceState(
            hasSim,
            hasSim ? phoneNumber : null,
            callState,
            GetCallDisposition(ent.Owner),
            peerNumber,
            conversations,
            error);

        _userInterface.SetUiState(ent.Owner, PhoneUiKey.Key, state);
    }

    private bool TryGetPhoneIdentity(
        Entity<PhoneComponent> phone,
        out Entity<SimCardComponent> sim,
        out uint number)
    {
        number = default;
        if (!TryGetSim(phone, out sim) || sim.Comp.Number is not { } simNumber)
            return false;

        number = simNumber;
        return true;
    }

    private bool TryFindSimByNumber(uint number, out Entity<SimCardComponent> sim)
    {
        var query = EntityQueryEnumerator<SimCardComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (component.Number != number || TerminatingOrDeleted(uid))
                continue;

            sim = (uid, component);
            return true;
        }

        sim = default;
        return false;
    }

    private bool TryFindPhoneUsingSim(EntityUid simUid, out Entity<PhoneComponent> phone)
    {
        var query = EntityQueryEnumerator<PhoneComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (TerminatingOrDeleted(uid) || component.SimSlot.Item != simUid)
                continue;

            phone = (uid, component);
            return true;
        }

        phone = default;
        return false;
    }

    private bool TryFindOnlinePhone(Entity<SimCardComponent> sim, out Entity<PhoneComponent> phone)
    {
        if (TryFindPhoneUsingSim(sim.Owner, out phone) &&
            TryGetPhoneIdentity(phone, out var inserted, out _) &&
            inserted.Owner == sim.Owner)
        {
            return true;
        }

        phone = default;
        return false;
    }

    private void EndCallForLifecycle(Entity<PhoneComponent> phone)
    {
        if (!TryComp<TelephoneComponent>(phone.Owner, out var telephone) ||
            telephone.CurrentState == TelephoneState.Idle)
        {
            return;
        }

        MarkCallEnded(phone.Owner, telephone);
        _telephone.EndTelephoneCalls((phone.Owner, telephone));
    }

    private void TerminateCallForLifecycle(Entity<PhoneComponent> phone)
    {
        if (!TryComp<TelephoneComponent>(phone.Owner, out var telephone) ||
            telephone.CurrentState == TelephoneState.Idle)
        {
            return;
        }

        MarkCallEnded(phone.Owner, telephone);
        _telephone.TerminateTelephoneCalls((phone.Owner, telephone));
    }
}
