using Content.Goobstation.Shared.Wraith.SaltLines;
using Content.Server.Administration.Logs;
using Content.Server.Popups;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Database;
using Content.Shared.Interaction;
using Robust.Shared.Map.Components;

namespace Content.Goobstation.Server.Wraith.SaltLines;

public sealed class SaltLineSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SaltLineComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<SaltLineComponent, AnchorStateChangedEvent>(OnAnchorChanged);
        SubscribeLocalEvent<SaltLinePlacerComponent, AfterInteractEvent>(OnSaltLineAfterInteract);

        SubscribeLocalEvent<ConsumeOnSaltLineComponent, AttemptSaltLineEvent>(OnAttemptSaltLine);
    }

    private void OnMapInit(Entity<SaltLineComponent> ent, ref MapInitEvent args) =>
        UpdateAppearance(ent);

    private void OnAnchorChanged(Entity<SaltLineComponent> ent, ref AnchorStateChangedEvent args)
    {
        if (!args.Anchored)
            return;

        UpdateAppearance(ent);
        UpdateNeighbors(ent);
    }

    private void OnSaltLineAfterInteract(Entity<SaltLinePlacerComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach)
            return;

        if (!TryComp<MapGridComponent>(_transform.GetGrid(args.ClickLocation), out var grid))
            return;

        var gridUid = _transform.GetGrid(args.ClickLocation)!.Value;
        var snapPos = _map.TileIndicesFor((gridUid, grid), args.ClickLocation);

        var anchored = _map.GetAnchoredEntitiesEnumerator(gridUid, grid, snapPos);
        while (anchored.MoveNext(out var entity))
        {
            if (HasComp<SaltLineComponent>(entity.Value)) // dont place in same tile
                return;
        }

        var ev = new AttemptSaltLineEvent();
        ev.User = args.User;
        RaiseLocalEvent(ent.Owner, ref ev);

        if (ev.Cancelled)
            return;

        var newSaltLine = Spawn(ent.Comp.SaltLine, _map.GridTileToLocal(gridUid, grid, snapPos));
        _adminLogger.Add(LogType.Action, LogImpact.Low,
            $"{ToPrettyString(args.User)} placed {ToPrettyString(newSaltLine):saltline} at {Transform(newSaltLine).Coordinates}");
        args.Handled = true;
    }

    private void OnAttemptSaltLine(Entity<ConsumeOnSaltLineComponent> ent, ref AttemptSaltLineEvent args)
    {
        if (!_solution.TryGetSolution(ent.Owner, "food", out var sol, false))
        {
            args.Cancelled = true;
            return;
        }
        var reagentsalt = "TableSalt";
        var solution = sol.Value;
        var saltAmount = solution.Comp.Solution.GetTotalPrototypeQuantity(reagentsalt);
        
        if (saltAmount < ent.Comp.Amount)
        {
            _popupSystem.PopupEntity(Loc.GetString("consume-on-salt-line-component-not-enough-salt-message"), ent.Owner, args.User);
            args.Cancelled = true;
            return;
        }
        _solution.RemoveReagent(solution, reagentsalt, ent.Comp.Amount);
    }

    #region Helpers
    private void UpdateAppearance(Entity<SaltLineComponent> ent)
    {
        var transform = Transform(ent.Owner);
        if (!TryComp<MapGridComponent>(transform.GridUid, out var grid) || transform.GridUid == null)
            return;

        var mask = SaltLineVisDirFlags.None;
        var tile = _map.TileIndicesFor((transform.GridUid.Value, grid), transform.Coordinates);

        var directions = new[]
        {
            (new Vector2i(0, 1), SaltLineVisDirFlags.North),
            (new Vector2i(0, -1), SaltLineVisDirFlags.South),
            (new Vector2i(1, 0), SaltLineVisDirFlags.East),
            (new Vector2i(-1, 0), SaltLineVisDirFlags.West)
        };

        foreach (var (offset, flag) in directions)
        {
            var checkTile = tile + offset;
            var anchored = _map.GetAnchoredEntitiesEnumerator(transform.GridUid.Value, grid, checkTile);

            while (anchored.MoveNext(out var entity))
            {
                if (HasComp<SaltLineComponent>(entity.Value))
                {
                    mask |= flag;
                    break;
                }
            }
        }

        _appearance.SetData(ent.Owner, SaltLineVisuals.ConnectedMask, mask);
    }

    private void UpdateNeighbors(EntityUid uid)
    {
        var transform = Transform(uid);
        if (!TryComp<MapGridComponent>(transform.GridUid, out var grid) || transform.GridUid == null)
            return;

        var tile = _map.TileIndicesFor((transform.GridUid.Value, grid), transform.Coordinates);
        var offsets = new[] { new Vector2i(0, 1), new Vector2i(0, -1), new Vector2i(1, 0), new Vector2i(-1, 0) };

        foreach (var offset in offsets)
        {
            var checkTile = tile + offset;
            var anchored = _map.GetAnchoredEntitiesEnumerator(transform.GridUid.Value, grid, checkTile);

            while (anchored.MoveNext(out var entity))
            {
                if (!TryComp<SaltLineComponent>(entity.Value, out var comp))
                    continue;

                UpdateAppearance((entity.Value, comp));
            }
        }
    }
    #endregion
}
