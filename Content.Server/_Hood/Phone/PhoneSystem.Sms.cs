// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration.Logs;
using Content.Server.PDA.Ringer;
using Content.Shared._Hood.Phone;
using Content.Shared.Database;
using Content.Shared.PDA.Ringer;
using Robust.Shared.Timing;

namespace Content.Server._Hood.Phone;

public sealed partial class PhoneSystem
{
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly RingerSystem _ringer = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private sealed class SimSmsSession
    {
        public readonly Dictionary<uint, List<PhoneSmsMessage>> Conversations = [];
        public readonly Dictionary<Guid, PhoneOperationError> OutgoingResults = [];
        public readonly HashSet<(uint SenderNumber, Guid RequestId)> Received = [];
    }

    private readonly Dictionary<EntityUid, SimSmsSession> _smsSessions = [];

    /// <summary>
    /// Attempts one SMS operation. A request ID is processed at most once for the lifetime of the sending SIM.
    /// </summary>
    public PhoneOperationError SendSms(
        Entity<PhoneComponent> source,
        uint recipientNumber,
        string? content,
        Guid requestId,
        EntityUid actor)
    {
        if (!TryGetPhoneIdentity(source, out var senderSim, out var senderNumber))
            return PhoneOperationError.NoSim;

        var senderSession = GetSmsSession(senderSim.Owner);
        if (senderSession.OutgoingResults.TryGetValue(requestId, out var previousResult))
            return previousResult;

        if (requestId == Guid.Empty)
            return RememberResult(senderSession, requestId, PhoneOperationError.InvalidRequest);

        var sanitized = (content ?? string.Empty).Trim();
        if (sanitized.Length == 0)
            return RememberResult(senderSession, requestId, PhoneOperationError.EmptyMessage);

        if (sanitized.Length > PhoneSmsMessage.MaxContentLength)
            sanitized = sanitized[..PhoneSmsMessage.MaxContentLength];

        if (recipientNumber == senderNumber)
            return RememberResult(senderSession, requestId, PhoneOperationError.SelfTarget);

        if (!TryFindSimByNumber(recipientNumber, out var recipientSim))
            return RememberResult(senderSession, requestId, PhoneOperationError.InvalidNumber);

        var message = new PhoneSmsMessage(
            requestId,
            _timing.CurTime,
            sanitized,
            senderNumber,
            recipientNumber);

        if (!TryFindOnlinePhone(recipientSim, out var recipientPhone))
        {
            message.DeliveryFailed = true;
            AddMessage(senderSession, recipientNumber, message);
            _adminLogger.Add(
                LogType.Chat,
                LogImpact.Low,
                $"{ToPrettyString(actor):user} attempted Hood SMS H-{senderNumber:D4} -> H-{recipientNumber:D4}: {sanitized} [OFFLINE]");

            return RememberResult(senderSession, requestId, PhoneOperationError.Offline);
        }

        AddMessage(senderSession, recipientNumber, message);

        var recipientSession = GetSmsSession(recipientSim.Owner);
        if (recipientSession.Received.Add((senderNumber, requestId)))
        {
            AddMessage(recipientSession, senderNumber, message);

            if (HasComp<RingerComponent>(recipientPhone.Owner))
                _ringer.RingerPlayRingtone(recipientPhone.Owner);

            UpdateUi(recipientPhone);
        }

        _adminLogger.Add(
            LogType.Chat,
            LogImpact.Low,
            $"{ToPrettyString(actor):user} sent Hood SMS H-{senderNumber:D4} -> H-{recipientNumber:D4}: {sanitized}");

        return RememberResult(senderSession, requestId, PhoneOperationError.None);
    }

    /// <summary>
    /// Returns a copy suitable for tests and other read-only server consumers.
    /// </summary>
    public IReadOnlyList<PhoneSmsMessage> GetConversation(Entity<SimCardComponent> sim, uint otherNumber)
    {
        if (!_smsSessions.TryGetValue(sim.Owner, out var session) ||
            !session.Conversations.TryGetValue(otherNumber, out var messages))
        {
            return Array.Empty<PhoneSmsMessage>();
        }

        return messages.ToArray();
    }

    private static PhoneOperationError RememberResult(
        SimSmsSession session,
        Guid requestId,
        PhoneOperationError result)
    {
        session.OutgoingResults[requestId] = result;
        return result;
    }

    private static void AddMessage(SimSmsSession session, uint conversation, PhoneSmsMessage message)
    {
        if (!session.Conversations.TryGetValue(conversation, out var messages))
        {
            messages = [];
            session.Conversations.Add(conversation, messages);
        }

        messages.Add(message);
    }

    private SimSmsSession GetSmsSession(EntityUid sim)
    {
        if (_smsSessions.TryGetValue(sim, out var session))
            return session;

        session = new SimSmsSession();
        _smsSessions.Add(sim, session);
        return session;
    }

    private Dictionary<uint, List<PhoneSmsMessage>> BuildConversationSnapshot(EntityUid sim)
    {
        var result = new Dictionary<uint, List<PhoneSmsMessage>>();
        if (!_smsSessions.TryGetValue(sim, out var session))
            return result;

        foreach (var (number, messages) in session.Conversations)
        {
            result.Add(number, new List<PhoneSmsMessage>(messages));
        }

        return result;
    }

    private void RemoveSmsSession(EntityUid sim)
    {
        _smsSessions.Remove(sim);
    }

    private void ClearSmsSessions()
    {
        _smsSessions.Clear();
    }
}
