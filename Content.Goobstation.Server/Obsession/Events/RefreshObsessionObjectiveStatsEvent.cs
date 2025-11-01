using Content.Goobstation.Shared.Obsession;
using Content.Shared.Mind;

namespace Content.Goobstation.Server.Obsession;

[ByRefEvent]
public record struct RefreshObsessionObjectiveStatsEvent(EntityUid MindId, MindComponent Mind, ObsessionInteraction Interaction, int Count);
