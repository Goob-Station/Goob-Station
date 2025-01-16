using Content.Shared.FixedPoint;
using Content.Shared.StationRecords;
using Robust.Client.Input;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.PrisonerId;

[Serializable, NetSerializable]
public sealed class StartPrisonerSentence(NetEntity uid) : EntityEventArgs
{
    public readonly NetEntity Uid = uid;
}

[Serializable, NetSerializable]
public sealed class SpawnedPrisonerId(NetEntity uid, FixedPoint2 time, uint key, NetEntity station) : EntityEventArgs
{
    public readonly NetEntity Uid = uid;
    public readonly FixedPoint2 Time = time;
    public readonly uint Key = key;
    public readonly NetEntity Station = station;
}
