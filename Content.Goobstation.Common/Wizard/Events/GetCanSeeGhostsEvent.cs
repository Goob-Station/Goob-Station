namespace Content.Goobstation.Common.Wizard.Events;

[ByRefEvent]
public record struct GetCanSeeGhostsEvent(EntityUid? Uid, bool Can = false, bool CheckIfForced = false);