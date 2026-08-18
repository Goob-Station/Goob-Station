// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Hood.Phone;
using Content.Shared.Telephone;

namespace Content.Server._Hood.Phone;

public sealed partial class PhoneSystem
{
    private readonly Dictionary<EntityUid, PhoneCallDisposition> _callDispositions = [];

    /// <summary>
    /// Resolves a dialed number through its live SIM and delegates call setup to the upstream telephone system.
    /// </summary>
    public PhoneOperationError Dial(Entity<PhoneComponent> source, uint recipientNumber, EntityUid actor)
    {
        if (!TryGetPhoneIdentity(source, out _, out var senderNumber))
            return PhoneOperationError.NoSim;

        if (recipientNumber == senderNumber)
            return PhoneOperationError.SelfTarget;

        if (!TryFindSimByNumber(recipientNumber, out var recipientSim))
            return PhoneOperationError.InvalidNumber;

        if (!TryFindOnlinePhone(recipientSim, out var recipientPhone) ||
            !TryComp<TelephoneComponent>(source.Owner, out var sourceTelephone) ||
            !TryComp<TelephoneComponent>(recipientPhone.Owner, out var recipientTelephone))
        {
            return PhoneOperationError.Offline;
        }

        if (sourceTelephone.CurrentState != TelephoneState.Idle ||
            recipientTelephone.CurrentState != TelephoneState.Idle)
        {
            return PhoneOperationError.Busy;
        }

        if (!_telephone.IsSourceAbleToReachReceiver(
                (source.Owner, sourceTelephone),
                (recipientPhone.Owner, recipientTelephone)))
        {
            return PhoneOperationError.Offline;
        }

        ClearCallDisposition(source.Owner);
        ClearCallDisposition(recipientPhone.Owner);
        _telephone.CallTelephone(
            (source.Owner, sourceTelephone),
            (recipientPhone.Owner, recipientTelephone),
            actor);

        return sourceTelephone.CurrentState is TelephoneState.Calling or TelephoneState.InCall
            ? PhoneOperationError.None
            : PhoneOperationError.Offline;
    }

    public PhoneOperationError AcceptCall(Entity<PhoneComponent> receiver, EntityUid actor)
    {
        if (!TryGetPhoneIdentity(receiver, out _, out _))
            return PhoneOperationError.NoSim;

        if (!TryComp<TelephoneComponent>(receiver.Owner, out var telephone) ||
            telephone.CurrentState != TelephoneState.Ringing)
        {
            return PhoneOperationError.InvalidState;
        }

        if (!HasOnlineCallPeer(telephone))
        {
            _telephone.EndTelephoneCalls((receiver.Owner, telephone));
            return PhoneOperationError.Offline;
        }

        ClearCallDisposition(receiver.Owner);
        foreach (var peer in telephone.LinkedTelephones)
            ClearCallDisposition(peer.Owner);

        _telephone.AnswerTelephone((receiver.Owner, telephone), actor);
        return telephone.CurrentState == TelephoneState.InCall
            ? PhoneOperationError.None
            : PhoneOperationError.InvalidState;
    }

    public PhoneOperationError RejectCall(Entity<PhoneComponent> receiver)
    {
        if (!TryGetPhoneIdentity(receiver, out _, out _))
            return PhoneOperationError.NoSim;

        if (!TryComp<TelephoneComponent>(receiver.Owner, out var telephone) ||
            telephone.CurrentState != TelephoneState.Ringing)
        {
            return PhoneOperationError.InvalidState;
        }

        SetCallDisposition(receiver.Owner, PhoneCallDisposition.Ended);
        foreach (var peer in telephone.LinkedTelephones)
            SetCallDisposition(peer.Owner, PhoneCallDisposition.Rejected);

        _telephone.EndTelephoneCalls((receiver.Owner, telephone));
        return PhoneOperationError.None;
    }

    public PhoneOperationError Hangup(Entity<PhoneComponent> phone)
    {
        if (!TryGetPhoneIdentity(phone, out _, out _))
            return PhoneOperationError.NoSim;

        if (!TryComp<TelephoneComponent>(phone.Owner, out var telephone) ||
            telephone.CurrentState is TelephoneState.Idle or TelephoneState.EndingCall)
        {
            return PhoneOperationError.InvalidState;
        }

        MarkCallEnded(phone.Owner, telephone);
        _telephone.EndTelephoneCalls((phone.Owner, telephone));
        return PhoneOperationError.None;
    }

    private bool HasOnlineCallPeer(TelephoneComponent telephone)
    {
        if (telephone.LinkedTelephones.Count != 1)
            return false;

        foreach (var peer in telephone.LinkedTelephones)
        {
            if (!TryComp<PhoneComponent>(peer.Owner, out var peerPhone) ||
                !TryGetPhoneIdentity((peer.Owner, peerPhone), out var peerSim, out _) ||
                !TryFindOnlinePhone(peerSim, out var onlinePhone) ||
                onlinePhone.Owner != peer.Owner)
            {
                return false;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns the last completed call outcome for UI and diagnostics.
    /// The outcome is cleared when another call starts or is accepted.
    /// </summary>
    public PhoneCallDisposition GetCallDisposition(EntityUid phone)
    {
        return _callDispositions.GetValueOrDefault(phone);
    }

    private void SetCallDisposition(EntityUid phone, PhoneCallDisposition disposition)
    {
        _callDispositions[phone] = disposition;
    }

    private void ClearCallDisposition(EntityUid phone)
    {
        _callDispositions.Remove(phone);
    }

    private void RemoveCallDisposition(EntityUid phone)
    {
        _callDispositions.Remove(phone);
    }

    private void ClearCallDispositions()
    {
        _callDispositions.Clear();
    }

    private void MarkCallEnded(EntityUid phone, TelephoneComponent telephone)
    {
        SetCallDisposition(phone, PhoneCallDisposition.Ended);
        foreach (var peer in telephone.LinkedTelephones)
            SetCallDisposition(peer.Owner, PhoneCallDisposition.Ended);
    }
}
