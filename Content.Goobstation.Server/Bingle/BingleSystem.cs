// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 unknown <Administrator@DESKTOP-PMRIVVA.kommune.indresogn.no>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Goobstation.Common.Bingle;
using Content.Goobstation.Shared.Bingle;
using Content.Server.Flash.Components;
using Content.Server.Polymorph.Components;
using Content.Server.Polymorph.Systems;
using Content.Shared.CombatMode;
using Content.Shared.Damage;
using Content.Shared.Interaction.Events;
using Content.Shared.Maps;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Bingle;

public sealed class BingleSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefManager = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly TileSystem _tileSystem = default!;

    private const float DamageFrequency = 3.0f;
    private const float MaxDistance = 15.0f;

    private readonly DamageSpecifier _damage = new()
    {
        DamageDict = new Dictionary<string, Content.Goobstation.Maths.FixedPoint.FixedPoint2>
        {
            { "Cellular", 5 },
        }
    };

    private readonly DamageSpecifier _healing = new()
    {
        DamageDict = new Dictionary<string, Content.Goobstation.Maths.FixedPoint.FixedPoint2>
        {
            { "Cellular", -5 },
        }
    };
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BingleComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<BingleComponent, AttackAttemptEvent>(OnAttackAttempt);
        SubscribeLocalEvent<BingleComponent, Shared.Overlays.ToggleNightVisionEvent>(OnNightvision);
        SubscribeLocalEvent<BingleComponent, ToggleCombatActionEvent>(OnCombatToggle);
    }

    private void OnMapInit(EntityUid uid, BingleComponent component, MapInitEvent args)
    {
        var cords = Transform(uid).Coordinates;
        if (!cords.IsValid(EntityManager) || cords.Position == Vector2.Zero)
            return;
        if (MapId.Nullspace == Transform(uid).MapID)
            return;

        if (component.Prime)
        {
            component.MyPit = Spawn("BinglePit", cords);
            SpawnBingleFloorBelow(component.MyPit.Value);
        }
        else
        {
            var query = EntityQueryEnumerator<BinglePitComponent>();
            while (query.MoveNext(out var queryUid, out var _))
            {
                if (cords == Transform(queryUid).Coordinates)
                    component.MyPit = queryUid;
            }
        }
    }

    //ran by the pit to upgrade bingle damage
    public void UpgradeBingle(EntityUid uid, BingleComponent component)
    {
        if (component.Upgraded)
            return;

        var polyComp = EnsureComp<PolymorphableComponent>(uid);
        _polymorph.CreatePolymorphAction("BinglePolymorph",(uid, polyComp ));

        _popup.PopupEntity(Loc.GetString("bingle-upgrade-success"), uid, uid);
        component.Upgraded = true;
    }

    private void OnAttackAttempt(EntityUid uid, BingleComponent component, AttackAttemptEvent args)
    {
        //Prevent Friendly Bingle fire
        if (HasComp<BinglePitComponent>(args.Target) || HasComp<BingleComponent>(args.Target))
            args.Cancel();
    }

    private void OnNightvision(EntityUid uid, BingleComponent component, Shared.Overlays.ToggleNightVisionEvent args)
    {
        if (!TryComp<FlashImmunityComponent>(uid, out var flashComp))
            return;

        flashComp.Enabled = !flashComp.Enabled;
    }

    private void OnCombatToggle(EntityUid uid, BingleComponent component, ToggleCombatActionEvent args)
    {
        if (!TryComp<CombatModeComponent>(uid, out var combat))
            return;
        _appearance.SetData(uid, BingleVisual.Combat, combat.IsInCombatMode);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var bingleQuery = EntityQueryEnumerator<BingleComponent, MobStateComponent>();
        while (bingleQuery.MoveNext(out var ent, out var bingleComp, out var mobStateComponent))
        {
            if (_mobStateSystem.IsDead(ent, mobStateComponent))
                continue;

            bingleComp.NextDamageCheck += frameTime;

            if (bingleComp.NextDamageCheck < DamageFrequency)
                continue;

            bingleComp.NextDamageCheck -= DamageFrequency;

            var xform = Transform(ent);
            if (xform.GridUid == null)
                continue;

            if (!TryComp<MapGridComponent>(xform.GridUid, out var mapGrid))
                continue;

            var position = xform.Coordinates;
            var gridPos = mapGrid.WorldToTile(position.ToMapPos(EntityManager, _transform));

            // Check if there's a bingle floor tile within range
            if (!IsNearBingleFloor(xform.GridUid.Value, mapGrid, gridPos))
            {
                _popup.PopupEntity(Loc.GetString("bingle-distance-damage"), ent, ent, PopupType.LargeCaution);
                _damageableSystem.TryChangeDamage(ent, _damage);
            }
            else
            {
                _damageableSystem.TryChangeDamage(ent, _healing);
            }
        }
    }

    private bool IsNearBingleFloor(EntityUid gridUid, MapGridComponent mapGrid, Vector2i centerPos)
    {
        var maxTileDistance = (int)Math.Ceiling(MaxDistance);

        for (var x = -maxTileDistance; x <= maxTileDistance; x++)
        {
            for (var y = -maxTileDistance; y <= maxTileDistance; y++)
            {
                var checkPos = centerPos + new Vector2i(x, y);
                var distance = (checkPos - centerPos).Length;

                if (distance > MaxDistance)
                    continue;

                var tileRef = mapGrid.GetTileRef(checkPos);
                var tileDef = tileRef.Tile.GetContentTileDefinition(_tileDefManager);

                if (tileDef.ID == "FloorBingle")
                    return true;
            }
        }

        return false;
    }

    private void SpawnBingleFloorBelow(EntityUid pitUid)
    {
        var xform = Transform(pitUid);
        if (xform.GridUid == null || !TryComp<MapGridComponent>(xform.GridUid, out var mapGrid))
            return;

        var gridPos = mapGrid.WorldToTile(xform.Coordinates.ToMapPos(EntityManager, _transform));
        var tileRef = mapGrid.GetTileRef(gridPos);
        var bingleFloorTile = (ContentTileDefinition)_tileDefManager["FloorBingle"];
        
        _tileSystem.ReplaceTile(tileRef, bingleFloorTile);
    }
}
