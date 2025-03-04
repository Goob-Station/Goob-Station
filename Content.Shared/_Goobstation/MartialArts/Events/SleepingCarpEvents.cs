namespace Content.Shared._Goobstation.MartialArts.Events;

public sealed class SleepingCarpGnashingTeethPerformedEvent : EntityEventArgs
{
}

public sealed class SleepingCarpKneeHaulPerformedEvent : EntityEventArgs
{
}

public sealed class SleepingCarpCrashingWavesPerformedEvent : EntityEventArgs
{
}

public sealed class SleepingCarpSaying(LocId saying) : EntityEventArgs
{
    public LocId Saying = saying;
};
