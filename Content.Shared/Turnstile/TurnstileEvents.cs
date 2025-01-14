using System.Numerics;
using Robust.Client.GameObjects;

using Robust.Shared.Serialization;

namespace Content.Shared.Turnstile;

[Serializable, NetSerializable]
public sealed class StartTurnstileEvent : EntityEventArgs
{
    public readonly NetEntity Uid;

    public StartTurnstileEvent(NetEntity  uid)
    {
        Uid = uid;
    }
}

[Serializable, NetSerializable]
public sealed class BadTurnstileEvent : EntityEventArgs
{
    public readonly NetEntity Uid;

    public BadTurnstileEvent(NetEntity  uid)
    {
        Uid = uid;
    }
}
