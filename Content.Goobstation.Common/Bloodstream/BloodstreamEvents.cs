namespace Content.Goobstation.Common.Bloodstream;

public sealed class StoppedTakingBloodlossDamageEvent : EntityEventArgs;

public sealed class GetBloodlossDamageMultiplierEvent(float multiplier = 1f) : EntityEventArgs
{
    public float Multiplier = multiplier;
}
