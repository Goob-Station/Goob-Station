using Robust.Shared.Player;

namespace Content.Goobstation.Common.Wizard.Events;

[ByRefEvent]
public record struct GetDeadchatAdditionalHearersEvent(Filter Filter);