using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Lathe;

/// <summary>
///     Sent to the server when a client resets the queue
/// </summary>
[Serializable, NetSerializable]
public sealed class LatheQueueResetMessage : BoundUserInterfaceMessage;
