using Content.Goobstation.Shared.Obsession;
using Content.Shared.Mind;

namespace Content.Goobstation.Server.Obsession;

[ByRefEvent]
public record struct ObsessionTargetDiedEvent(EntityUid MindId, MindComponent Mind, bool Handled = false);
