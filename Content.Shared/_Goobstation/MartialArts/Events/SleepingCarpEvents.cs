using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.MartialArts.Events;

[Serializable, NetSerializable, DataDefinition]
public sealed partial class SleepingCarpGnashingTeethPerformedEvent : EntityEventArgs;

[Serializable, NetSerializable, DataDefinition]
public sealed partial class SleepingCarpKneeHaulPerformedEvent : EntityEventArgs;

[Serializable, NetSerializable, DataDefinition]
public sealed partial class SleepingCarpCrashingWavesPerformedEvent : EntityEventArgs;

[Serializable,NetSerializable]
public sealed class SleepingCarpSaying(LocId saying) : EntityEventArgs
{
    public LocId Saying = saying;
};
