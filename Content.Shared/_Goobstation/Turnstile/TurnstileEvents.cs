using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Turnstile;

[Serializable, NetSerializable]
public sealed class StartTurnstileEvent(NetEntity uid) : EntityEventArgs
{
    public readonly NetEntity Uid = uid;
}

[Serializable, NetSerializable]
public sealed class BadTurnstileEvent(NetEntity uid) : EntityEventArgs
{
    public readonly NetEntity Uid = uid;
}
