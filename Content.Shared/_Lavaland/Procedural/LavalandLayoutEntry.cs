using System.Numerics;
using Robust.Shared.Utility;

namespace Content.Shared._Lavaland.Procedural;

[DataRecord]
public partial record struct LavalandLayoutEntry(
    ResPath GridPath,
    Vector2 Position,
    LocId Name);
