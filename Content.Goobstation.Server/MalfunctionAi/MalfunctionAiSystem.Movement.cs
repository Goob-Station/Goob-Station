// SPDX-FileCopyrightText: 2026 Jonikibaka <153797633+Jonikibaka@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Light.Components;
using Content.Server.Mind;
using Content.Server.Pinpointer;
using Content.Server.Power.Components;
using Content.Shared.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Radio.Components;
using Content.Server.Silicons.Laws;
using Content.Server.Station.Systems;
using Content.Server.Store.Systems;
using Content.Shared.SurveillanceCamera.Components;
using Content.Server.VoiceMask;
using Content.Goobstation.Shared.MalfunctionAi;
using Content.Goobstation.Shared.Overlays;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.Chat.RadioIconsEvents;
using Content.Shared.Speech;
using Content.Shared.Speech.Components;
using Content.Shared.VoiceMask;
using Robust.Shared.Player;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Electrocution;
using Content.Shared.Maps;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Physics;
using Content.Shared.StationAi;
using Content.Shared.Turrets;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Popups;
using Content.Shared.RCD.Components;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Content.Shared.Verbs;
using System.Numerics;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.MalfunctionAi;

public sealed partial class MalfunctionAiSystem
{
    private void OnGyroscope(Entity<MalfunctionAiComponent> ent, ref MalfGyroscopeEvent args)
    {
        if (args.Handled || !TryComp<MalfGyroscopeComponent>(ent.Owner, out var gyro))
            return;

        // Only works while the brain actually sits in a core (not carded).
        if (!_stationAi.TryGetCore(ent.Owner, out var maybeCore) || maybeCore.Comp == null)
        {
            _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-gyroscope-no-core"), ent.Owner);
            return;
        }

        var core = maybeCore.Owner;
        var coreXform = Transform(core);
        if (coreXform.GridUid is not { } gridUid || !TryComp<MapGridComponent>(gridUid, out var grid))
        {
            _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-gyroscope-blocked"), ent.Owner);
            return;
        }

        // One tile per use, towards the click (8-directional).
        var coreTile = _map.TileIndicesFor(gridUid, grid, coreXform.Coordinates);
        var targetTile = _map.TileIndicesFor(gridUid, grid, args.Target);
        var offset = new Vector2i(Math.Clamp(targetTile.X - coreTile.X, -1, 1),
            Math.Clamp(targetTile.Y - coreTile.Y, -1, 1));

        if (offset == Vector2i.Zero)
        {
            _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-gyroscope-blocked"), ent.Owner);
            return;
        }

        var destination = coreTile + offset;

        // The destination must be a real floor tile with no walls, windows, tables or machines on it.
        if (!_map.TryGetTileRef(gridUid, grid, destination, out var tileRef)
            || tileRef.Tile.IsEmpty
            || _turfs.IsTileBlocked(gridUid, destination, CollisionGroup.FullTileMask, grid))
        {
            _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-gyroscope-blocked"), ent.Owner);
            return;
        }

        var wasAnchored = coreXform.Anchored;
        if (wasAnchored)
            _transform.Unanchor(core, coreXform);

        _transform.SetCoordinates(core, _map.GridTileToLocal(gridUid, grid, destination));

        if (wasAnchored)
            _transform.AnchorEntity(core, coreXform);

        // Anything alive on the destination tile gets crushed under the core's bulk:
        // bodied creatures are gibbed outright (no corpse left to shove aside), the rest
        // take massive blunt damage.
        var crushed = 0;
        foreach (var victim in _lookup.GetLocalEntitiesIntersecting(gridUid, destination, gridComp: grid))
        {
            if (victim == core || victim == ent.Owner || !HasComp<MobStateComponent>(victim))
                continue;

            _popups.PopupEntity(Loc.GetString("malfunction-ai-popup-gyroscope-crush", ("target", Name(victim))), victim, PopupType.LargeCaution);

            if (TryComp<BodyComponent>(victim, out var body))
                _body.GibBody(victim, gibOrgans: true, body: body);
            else
                _damageable.TryChangeDamage(victim, gyro.CrushDamage, ignoreResistances: true, origin: ent.Owner);

            crushed++;
        }

        if (crushed > 0)
            _audio.PlayPvs(gyro.CrushSound, core);

        _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-gyroscope-success"), ent.Owner);
        args.Handled = true;
    }

    private void OnShuntToApc(Entity<MalfunctionAiComponent> ent, ref MalfShuntToApcEvent args)
    {
        if (args.Handled)
            return;

        // Only from inside a core: no escaping out of an intellicard.
        if (!_stationAi.TryGetCore(ent.Owner, out var maybeCore) || maybeCore.Comp == null)
        {
            _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-gyroscope-no-core"), ent.Owner);
            return;
        }

        EntityUid? apc = null;
        foreach (var candidate in _lookup.GetEntitiesInRange(args.Target, 0.75f))
        {
            if (!HasComp<ApcComponent>(candidate))
                continue;

            apc = candidate;
            break;
        }

        if (apc == null)
        {
            _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-invalid-target"), ent.Owner);
            return;
        }

        // Consciousness can only run on APCs the AI has already hacked.
        if (!HasComp<MalfHackedApcComponent>(apc.Value))
        {
            _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-shunt-not-hacked"), ent.Owner);
            return;
        }

        if (!_mind.TryGetMind(ent.Owner, out var mindId, out _))
            return;

        // Parented to the APC: if the APC is destroyed, the shunted presence dies with it.
        var shuntProto = CompOrNull<MalfShuntComponent>(ent.Owner)?.ShuntEntity ?? "MalfShuntedAi";
        var shunt = Spawn(shuntProto, new EntityCoordinates(apc.Value, Vector2.Zero));
        EnsureComp<MalfShuntedAiComponent>(shunt).Brain = ent.Owner;

        _mind.TransferTo(mindId, shunt);
        args.Handled = true;
    }

    private void OnReturnToCore(Entity<MalfShuntedAiComponent> ent, ref MalfReturnToCoreEvent args)
    {
        if (args.Handled)
            return;

        if (ent.Comp.Brain is not { } brain || !Exists(brain) || Deleted(brain))
        {
            _popups.PopupEntity(Loc.GetString("malfunction-ai-popup-return-no-core"), ent.Owner, ent.Owner);
            return;
        }

        if (!_mind.TryGetMind(ent.Owner, out var mindId, out _))
            return;

        _mind.TransferTo(mindId, brain);
        QueueDel(ent.Owner);
        args.Handled = true;
    }
}
