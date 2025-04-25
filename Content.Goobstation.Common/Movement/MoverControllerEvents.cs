using Robust.Shared.Map;

namespace Content.Goobstation.Common.Movement;

[ByRefEvent]
public readonly record struct MoverControllerCantMoveEvent;

[ByRefEvent]
public readonly record struct MoverControllerGetTileEvent(ITileDefinition? Tile);
