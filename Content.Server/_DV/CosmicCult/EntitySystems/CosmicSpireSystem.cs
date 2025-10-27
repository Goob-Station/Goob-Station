using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Components;
using Content.Server.Atmos.Piping.Unary.EntitySystems;
using Content.Server.Audio;
using Content.Server.Popups;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Piping.Unary.Components;
using Robust.Server.GameObjects;

namespace Content.Server._DV.CosmicCult.EntitySystems;

public sealed class CosmicSpireSystem : EntitySystem
{
    [Dependency] private readonly AmbientSoundSystem _ambient = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly CosmicCultRuleSystem _cosmicRule = default!;
    [Dependency] private readonly SharedPointLightSystem _lights = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly GasVentScrubberSystem _scrub = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CosmicSpireComponent, AnchorStateChangedEvent>(OnAnchorChanged);
        SubscribeLocalEvent<CosmicSpireComponent, AtmosDeviceUpdateEvent>(OnDeviceUpdated);
        SubscribeLocalEvent<CosmicSpireComponent, GasAnalyzerScanEvent>(OnSpireAnalyzed);
    }

    private void OnAnchorChanged(Entity<CosmicSpireComponent> ent, ref AnchorStateChangedEvent args)
    {
        if (args.Anchored)
        {
            ent.Comp.Enabled = true;
            UpdateSpireAppearance(ent, SpireStatus.On);
        }

        if (!args.Anchored)
        {
            ent.Comp.Enabled = false;
            UpdateSpireAppearance(ent, SpireStatus.Off);
        }

        _ambient.SetAmbience(ent, ent.Comp.Enabled);
        _lights.SetEnabled(ent, ent.Comp.Enabled);
    }

    private void OnDeviceUpdated(Entity<CosmicSpireComponent> ent, ref AtmosDeviceUpdateEvent args)
    {
        if (!ent.Comp.Enabled
            || args.Grid is not { } grid)
            return;

        var timeDelta = args.dt;
        var position = _transform.GetGridTilePositionOrDefault(ent.Owner);
        var environment = _atmos.GetTileMixture(grid, args.Map, position, true);
        var running = Drain(timeDelta, ent, environment);

        if (!running)
            return;

        var enumerator = _atmos.GetAdjacentTileMixtures(grid, position, false, true);

        while (enumerator.MoveNext(out var adjacent))
            Drain(timeDelta, ent, adjacent);

        if (ent.Comp.Storage.TotalMoles >= ent.Comp.DrainThreshHold)
        {
            _popup.PopupCoordinates(Loc.GetString("cosmiccult-spire-entropy"), Transform(ent).Coordinates);
            ent.Comp.Storage.Clear();
            Spawn(ent.Comp.SpawnVFX, Transform(ent).Coordinates);
            Spawn(ent.Comp.EntropyMote, Transform(ent).Coordinates);

            if (_cosmicRule.AssociatedGamerule(ent) is not { } cult)
                return;

            cult.Comp.EntropySiphoned++;
        }
    }

    private bool Drain(float timeDelta, Entity<CosmicSpireComponent> ent, GasMixture? tile)
    {
        return _scrub.Scrub(timeDelta,
            ent.Comp.DrainRate * _atmos.PumpSpeedup(),
            ScrubberPumpDirection.Scrubbing,
            ent.Comp.DrainGases,
            tile,
            ent.Comp.Storage);
    }

    private void OnSpireAnalyzed(Entity<CosmicSpireComponent> ent, ref GasAnalyzerScanEvent args)
    {
        args.GasMixtures ??= [];
        args.GasMixtures.Add((Name(ent), ent.Comp.Storage));
    }

    private void UpdateSpireAppearance(EntityUid uid, SpireStatus status) =>
        _appearance.SetData(uid, SpireVisuals.Status, status);
}
