using System.Linq;
using Content.Server.Actions;
using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Shared._EE.Shadowling;
using Content.Shared._EE.Shadowling.Components;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Robust.Server.GameObjects;
using Robust.Shared.Map;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// debug
/// </summary>
public sealed class ShadowlingAscendanceSystem : EntitySystem
{
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingAscendanceComponent, AscendanceEvent>(OnAscendance);
        SubscribeLocalEvent<ShadowlingAscendanceComponent, AscendanceDoAfterEvent>(OnAscendanceDoAfter);
    }

    private void OnAscendance(EntityUid uid, ShadowlingAscendanceComponent component, AscendanceEvent args)
    {
        if (!TileFree(uid))
        {
            _popup.PopupEntity(Loc.GetString("shadowling-ascendance-fail"), uid, uid, PopupType.MediumCaution);
            return;
        }


        var doAfter = new DoAfterArgs(
            EntityManager,
            uid,
            component.Duration,
            new AscendanceDoAfterEvent(),
            uid,
            used: args.Action)
        {
            BreakOnDamage = true,
            CancelDuplicate = true,
            BreakOnMove = true,
        };

        _doAfterSystem.TryStartDoAfter(doAfter);
    }

    private void OnAscendanceDoAfter(
        EntityUid uid,
        ShadowlingAscendanceComponent component,
        AscendanceDoAfterEvent args
    )
    {
        if (args.Cancelled)
            return;

        var cocoon = Spawn(component.EggProto, Transform(uid).Coordinates);
        var ascEgg = EntityManager.GetComponent<ShadowlingAscensionEggComponent>(cocoon);
        ascEgg.Creator = uid;
        _actions.RemoveAction(uid, args.Args.Used);
    }

    private bool TileFree(EntityUid uid)
    {
        // Check if tile is occupied
        var mapCoords = _transformSystem.GetMapCoordinates(uid);
        if (!_mapManager.TryFindGridAt(mapCoords, out var gridUid, out var grid))
            return false;

        if (_mapSystem.GetAnchoredEntities(gridUid, grid, mapCoords).Any())
            return false;

        return true;
    }
}
