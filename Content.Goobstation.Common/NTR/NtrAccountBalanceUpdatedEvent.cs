namespace Content.Goobstation.Common.NTR;

[ByRefEvent]
public record struct NtrAccountBalanceUpdatedEvent(EntityUid Uid, int Balance);
