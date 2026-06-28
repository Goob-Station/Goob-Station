using Robust.Shared.Serialization;

namespace Content.Shared._Lavaland.Audio;

/// <summary>
/// Says to start playing BossMusic bypassing the lack of projectile prediction.
/// </summary>
[Serializable, NetSerializable]
public sealed class StartBossMusicNetworkEvent : EntityEventArgs
{
    public readonly NetEntity Boss;

    public StartBossMusicNetworkEvent(NetEntity boss)
    {
        Boss = boss;
    }
}

/// <summary>
/// Same as above except ends it.
/// </summary>
[Serializable, NetSerializable]
public sealed class EndBossMusicNetworkEvent : EntityEventArgs;
