using Content.Shared.Abilities.Psionics;
using Content.Shared.Actions.Events;
using Robust.Shared.Map.Components;
using System.Linq;
using System.Numerics;

namespace Content.Server.Abilities.Psionics;

public sealed partial class AnomalyPowerSystem
{
    private void DoGasProducerAnomalyEffects(EntityUid uid, PsionicComponent component, AnomalyPowerActionEvent args, bool overcharged = false)
    {
        if (args.Gas is null)
            return;

        if (overcharged)
            GasProducerSupercrit(uid, component, args);
        else GasProducerPulse(uid, component, args);
    }

    private void GasProducerSupercrit(EntityUid uid, PsionicComponent component, AnomalyPowerActionEvent args)
    {
        var xform = Transform(uid);
        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
            return;

        var gas = args.Gas!.Value.SupercritReleasedGas;
        var mols = args.Gas!.Value.SupercritMoleAmount * component.CurrentAmplification;
        var radius = args.Gas!.Value.SupercritSpawnRadius * component.CurrentAmplification;
        var count = args.Gas!.Value.SupercritTileCount * component.CurrentDampening;
        var temp = args.Gas!.Value.SupercritTempChange * component.CurrentDampening;
        var localpos = xform.Coordinates.Position;
        var tilerefs = grid.GetLocalTilesIntersecting(
            new Box2(localpos + new Vector2(-radius, -radius), localpos + new Vector2(radius, radius))).ToArray();

        if (tilerefs.Length == 0)
            return;

        var mixture = _atmosphere.GetTileMixture((uid, xform), true);
        if (mixture != null)
        {
            mixture.AdjustMoles(gas, mols);
            mixture.Temperature += temp;
        }

        if (count == 0)
            return;

        _random.Shuffle(tilerefs);
        var amountCounter = 0;
        foreach (var tileref in tilerefs)
        {
            var mix = _atmosphere.GetTileMixture(xform.GridUid, xform.MapUid, tileref.GridIndices, true);
            amountCounter++;
            if (mix is not { })
                continue;

            mix.AdjustMoles(gas, mols);
            mix.Temperature += temp;

            if (amountCounter >= count)
                return;
        }
    }

    private void GasProducerPulse(EntityUid uid, PsionicComponent component, AnomalyPowerActionEvent args)
    {
        var xform = Transform(uid);
        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
            return;

        var gas = args.Gas!.Value.ReleasedGas;
        var mols = args.Gas!.Value.MoleAmount * component.CurrentAmplification;
        var radius = args.Gas!.Value.SpawnRadius * component.CurrentAmplification;
        var count = args.Gas!.Value.TileCount * component.CurrentDampening;
        var temp = args.Gas!.Value.TempChange * component.CurrentDampening;
        var localpos = xform.Coordinates.Position;
        var tilerefs = grid.GetLocalTilesIntersecting(
            new Box2(localpos + new Vector2(-radius, -radius), localpos + new Vector2(radius, radius))).ToArray();

        if (tilerefs.Length == 0)
            return;

        var mixture = _atmosphere.GetTileMixture((uid, xform), true);
        if (mixture != null)
        {
            mixture.AdjustMoles(gas, mols);
            mixture.Temperature += temp;
        }

        if (count == 0)
            return;

        _random.Shuffle(tilerefs);
        var amountCounter = 0;
        foreach (var tileref in tilerefs)
        {
            var mix = _atmosphere.GetTileMixture(xform.GridUid, xform.MapUid, tileref.GridIndices, true);
            amountCounter++;
            if (mix is not { })
                continue;

            mix.AdjustMoles(gas, mols);
            mix.Temperature += temp;

            if (amountCounter >= count)
                return;
        }
    }
}