using Robust.Shared.Map;

namespace Content.Goobstation.Shared.Communications;

[ByRefEvent]
public record struct TelecomConnectionOverrideEvent(MapId SourceMap, MapId TargetMap, bool Cancelled = true);
