using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.RoundIntro;

[Serializable, NetSerializable]
public sealed class RoundIntroEvent(NetEntity entity, string name, ProtoId<JobPrototype>? job, string station, bool lateJoin)
    : EntityEventArgs
{
    public NetEntity Entity = entity;
    public string Name = name;
    public ProtoId<JobPrototype>? Job = job;
    public string Station = station;
    public bool LateJoin = lateJoin;
}

[Serializable, NetSerializable]
public sealed class RoundIntroCoworkersEvent(List<RoundIntroCoworker> coworkers) : EntityEventArgs
{
    public List<RoundIntroCoworker> Coworkers = coworkers;
}

[Serializable, NetSerializable]
public readonly record struct RoundIntroCoworker(NetEntity Entity, string Name, ProtoId<JobPrototype>? Job);

[ByRefEvent]
public record struct ClientJoinGameAttemptEvent(bool Handled = false);
