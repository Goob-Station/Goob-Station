using Content.Goobstation.Shared.Wizard.Systems.Spells;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Spreader;
using Content.Shared.Maps;
using Robust.Server.GameObjects;
using Robust.Shared.Map;

namespace Content.Goobstation.Server.Wizard.Systems;

public sealed partial class SpellsSystem : SharedSpellsSystem
{
    [Dependency] private readonly TransformSystem _xform = default!;
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly SpreaderSystem _spreader = default!;
    [Dependency] private readonly SmokeSystem _smoke = default!;
}