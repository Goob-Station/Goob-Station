using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.RoundIntro;

[Serializable, NetSerializable]
public sealed class RoundIntroEvent(
    NetEntity entity,
    string name,
    ProtoId<JobPrototype>? job,
    ProtoId<AntagPrototype>? antag,
    bool antagBody,
    string? location,
    bool lateJoin) : EntityEventArgs
{
    public NetEntity Entity = entity;
    public string Name = name;
    public ProtoId<JobPrototype>? Job = job;
    public ProtoId<AntagPrototype>? Antag = antag;
    public bool AntagBody = antagBody;
    public string? Location = location;
    public bool LateJoin = lateJoin;
}

[Serializable, NetSerializable]
public sealed class RoundIntroCoworkersEvent(List<RoundIntroCoworker> coworkers, bool teammates) : EntityEventArgs
{
    public List<RoundIntroCoworker> Coworkers = coworkers;
    public bool Teammates = teammates;
}

[Serializable, NetSerializable]
public readonly record struct RoundIntroCoworker(
    NetEntity Entity,
    string Name,
    ProtoId<JobPrototype>? Job,
    ProtoId<AntagPrototype>? Antag);

[ByRefEvent]
public record struct ClientJoinGameAttemptEvent(bool Handled = false);
