using System.Linq;
using System.Numerics;
using Content.Server.WhiteDream.BloodCult.Spells;
using Content.Shared.Alert;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Server.WhiteDream.BloodCult.Empower;

public sealed class BloodCultEmpoweredSystem : EntitySystem
{
    [Dependency] private readonly ITileDefinitionManager _tileDefinition = default!;

    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly MapSystem _map = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodCultEmpoweredComponent, ComponentStartup>(OnEmpowerStartup);
        SubscribeLocalEvent<BloodCultEmpoweredComponent, ComponentShutdown>(OnEmpowerShutdown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        UpdateTimers(frameTime);
    }

    private void OnEmpowerStartup(Entity<BloodCultEmpoweredComponent> cultist, ref ComponentStartup args)
    {
        _alerts.ShowAlert(cultist, cultist.Comp.EmpoweredAlert);
        if (TryComp(cultist, out BloodCultSpellsHolderComponent? spellsHolder))
            spellsHolder.MaxSpells += cultist.Comp.ExtraSpells;
    }

    private void OnEmpowerShutdown(Entity<BloodCultEmpoweredComponent> cultist, ref ComponentShutdown args)
    {
        _alerts.ClearAlert(cultist, cultist.Comp.EmpoweredAlert);
        if (TryComp(cultist, out BloodCultSpellsHolderComponent? spellsHolder))
            spellsHolder.MaxSpells -= cultist.Comp.ExtraSpells;
    }

    private void UpdateTimers(float frameTime)
    {
        var query = EntityQueryEnumerator<BloodCultEmpoweredComponent>();
        while (query.MoveNext(out var uid, out var empowered))
        {
            if (!HasComp<BloodCultistComponent>(uid))
            {
                RemComp(uid, empowered);
                continue;
            }

            if (AnyCultTilesNearby((uid, empowered)))
            {
                empowered.TimeRemaining = empowered.DefaultTime;
                continue;
            }

            empowered.TimeRemaining -= TimeSpan.FromSeconds(frameTime);
            if (empowered.TimeRemaining <= TimeSpan.Zero)
                RemComp(uid, empowered);
        }
    }

    private bool AnyCultTilesNearby(Entity<BloodCultEmpoweredComponent> ent)
    {
        var transform = Transform(ent);
        var localpos = transform.Coordinates.Position;
        var gridUid = transform.GridUid;
        if (!gridUid.HasValue || !TryComp(gridUid, out MapGridComponent? grid))
            return false;

        var cultTile = _tileDefinition[ent.Comp.CultTile];

        var radius = ent.Comp.NearbyCultTileRadius;
        var tilesRefs = _map.GetLocalTilesIntersecting(
            gridUid.Value,
            grid,
            new Box2(localpos + new Vector2(-radius, -radius), localpos + new Vector2(radius, radius)));

        return tilesRefs.Any(tileRef => tileRef.Tile.TypeId == cultTile.TileId);
    }
}
