using Robust.Shared.Serialization;

namespace Content.Goobstation.Common.Wizard.Mutate;

[ByRefEvent]
public record struct HulkUncuffAttemptEvent(EntityUid cuffs)
{
    public EntityUid Cuffs = cuffs;
    public bool Success = false;
}
