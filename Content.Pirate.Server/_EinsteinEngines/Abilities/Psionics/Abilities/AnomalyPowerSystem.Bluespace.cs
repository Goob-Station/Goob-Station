using Content.Shared.Abilities.Psionics;
using Content.Shared.Actions.Events;
using Content.Shared.Mobs.Components;
using System.Linq;
using System.Numerics;
using Content.Shared.Database;
using Robust.Shared.Collections;

namespace Content.Server.Abilities.Psionics;

public sealed partial class AnomalyPowerSystem
{
    /// <summary>
    ///     This function handles emulating the effects of a "Bluespace Anomaly", using the caster as the "Anomaly",
    ///     while substituting their Psionic casting stats for "Severity and Stability".
    ///     Essentially, scramble the location of entities near the caster(possibly to include the caster).
    /// </summary>
    private void DoBluespaceAnomalyEffects(EntityUid uid, PsionicComponent component, AnomalyPowerActionEvent args, bool overcharged = false)
    {
        if (args.Bluespace is null)
            return;

        if (overcharged)
            BluespaceSupercrit(uid, component, args);
        else BluespacePulse(uid, component, args);
    }

    private void BluespaceSupercrit(EntityUid uid, PsionicComponent component, AnomalyPowerActionEvent args)
    {
        var xform = Transform(uid);
        var mapPos = _xform.GetWorldPosition(xform);
        var radius = args.Bluespace!.Value.SupercriticalTeleportRadius * component.CurrentAmplification;
        var gridBounds = new Box2(mapPos - new Vector2(radius, radius), mapPos + new Vector2(radius, radius));
        var mobs = new HashSet<Entity<MobStateComponent>>();
        _lookup.GetEntitiesInRange(xform.Coordinates, args.Bluespace!.Value.MaxShuffleRadius, mobs);
        foreach (var comp in mobs)
        {
            if (args.Bluespace!.Value.SupercritTeleportsCaster && comp.Owner == uid)
                continue;

            var ent = comp.Owner;
            var randomX = _random.NextFloat(gridBounds.Left, gridBounds.Right);
            var randomY = _random.NextFloat(gridBounds.Bottom, gridBounds.Top);

            var pos = new Vector2(randomX, randomY);

            _adminLogger.Add(LogType.Teleport, $"{ToPrettyString(ent)} has been teleported to {pos} by the supercritical {ToPrettyString(uid)} at {mapPos}");

            _xform.SetWorldPosition(ent, pos);
            _audio.PlayPvs(args.Bluespace!.Value.TeleportSound, ent);
        }
    }

    private void BluespacePulse(EntityUid uid, PsionicComponent component, AnomalyPowerActionEvent args)
    {
        var xformQuery = GetEntityQuery<TransformComponent>();
        var xform = xformQuery.GetComponent(uid);
        var range = args.Bluespace!.Value.MaxShuffleRadius * component.CurrentAmplification;
        var mobs = new HashSet<Entity<MobStateComponent>>();
        _lookup.GetEntitiesInRange(xform.Coordinates, range, mobs);
        var allEnts = new ValueList<EntityUid>(mobs.Select(m => m.Owner)) { uid };
        var coords = new ValueList<Vector2>();
        foreach (var ent in allEnts)
        {
            if (args.Bluespace!.Value.PulseTeleportsCaster && ent == uid
                || !xformQuery.TryGetComponent(ent, out var allXform))
                continue;

            coords.Add(_xform.GetWorldPosition(allXform));
        }

        _random.Shuffle(coords);
        for (var i = 0; i < allEnts.Count; i++)
        {
            _adminLogger.Add(LogType.Teleport, $"{ToPrettyString(allEnts[i])} has been shuffled to {coords[i]} by the {ToPrettyString(uid)} at {xform.Coordinates}");
            _xform.SetWorldPosition(allEnts[i], coords[i]);
        }
    }
}