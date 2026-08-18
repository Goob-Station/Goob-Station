// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Telephone;
using Robust.Shared.Serialization;

namespace Content.Shared._Hood.Phone;

[Serializable, NetSerializable]
public enum PhoneUiKey : byte
{
    Key,
}

/// <summary>
/// A result suitable for displaying to the user. The server is the only authority that produces these.
/// </summary>
[Serializable, NetSerializable]
public enum PhoneOperationError : byte
{
    None,
    NoSim,
    InvalidNumber,
    SelfTarget,
    Offline,
    Busy,
    InvalidState,
    EmptyMessage,
    InvalidRequest,
}

/// <summary>
/// The last completed call outcome, layered over the upstream telephone state machine.
/// It remains visible until a new call starts or is answered.
/// </summary>
[Serializable, NetSerializable]
public enum PhoneCallDisposition : byte
{
    None,
    Ended,
    Rejected,
}

/// <summary>
/// One immutable SMS entry. The request ID is supplied by the client and deduplicated by the server per SIM.
/// </summary>
[Serializable, NetSerializable, DataRecord]
public partial struct PhoneSmsMessage
{
    public const int MaxContentLength = 256;

    public Guid Id;
    public TimeSpan Timestamp;
    public string Content;
    public uint SenderNumber;
    public uint RecipientNumber;
    public bool DeliveryFailed;

    public PhoneSmsMessage(
        Guid id,
        TimeSpan timestamp,
        string content,
        uint senderNumber,
        uint recipientNumber,
        bool deliveryFailed = false)
    {
        Id = id;
        Timestamp = timestamp;
        Content = content;
        SenderNumber = senderNumber;
        RecipientNumber = recipientNumber;
        DeliveryFailed = deliveryFailed;
    }
}

/// <summary>
/// Complete server-owned state for the currently inserted SIM and the upstream telephone state machine.
/// </summary>
[Serializable, NetSerializable]
public sealed class PhoneBoundUserInterfaceState(
    bool hasSim,
    uint? phoneNumber,
    TelephoneState callState,
    PhoneCallDisposition callDisposition,
    uint? callPeerNumber,
    Dictionary<uint, List<PhoneSmsMessage>> conversations,
    PhoneOperationError error) : BoundUserInterfaceState
{
    public bool HasSim { get; } = hasSim;
    public uint? PhoneNumber { get; } = phoneNumber;
    public TelephoneState CallState { get; } = callState;
    public PhoneCallDisposition CallDisposition { get; } = callDisposition;
    public uint? CallPeerNumber { get; } = callPeerNumber;
    public Dictionary<uint, List<PhoneSmsMessage>> Conversations { get; } = conversations;
    public PhoneOperationError Error { get; } = error;
}

[Serializable, NetSerializable]
public sealed class PhoneSendSmsMessage(Guid requestId, uint recipientNumber, string content)
    : BoundUserInterfaceMessage
{
    public Guid RequestId { get; } = requestId;
    public uint RecipientNumber { get; } = recipientNumber;
    public string Content { get; } = content;
}

[Serializable, NetSerializable]
public sealed class PhoneDialMessage(uint recipientNumber) : BoundUserInterfaceMessage
{
    public uint RecipientNumber { get; } = recipientNumber;
}

[Serializable, NetSerializable]
public sealed class PhoneAcceptCallMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class PhoneRejectCallMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class PhoneHangupMessage : BoundUserInterfaceMessage;
