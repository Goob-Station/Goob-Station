using Content.Shared.StatusEffect;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using PhaseShiftedComponent = Content.Goobstation.Shared.BloodCult.Constructs.PhaseShift.PhaseShiftedComponent;

namespace Content.Goobstation.Server.BloodCult;

public sealed class ConstructActionsSystem : EntitySystem
{
    [Dependency] private readonly ITileDefinitionManager _tileDef = default!;

    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;

    private const string CultTileSpawnEffect = "CultTileSpawnEffect";

    public override void Initialize()
    {
        SubscribeLocalEvent<Goobstation.Shared.BloodCult.Spells.PlaceTileEntityEvent>(OnPlaceTileEntityEvent);
        SubscribeLocalEvent<Goobstation.Shared.BloodCult.Spells.PhaseShiftEvent>(OnPhaseShift);
    }

    private void OnPlaceTileEntityEvent(Goobstation.Shared.BloodCult.Spells.PlaceTileEntityEvent args)
    {
        if (args.Handled)
            return;

        if (args.Entity is { } entProtoId)
            Spawn(entProtoId, args.Target);

        if (args.TileId is { } tileId)
        {
            if (_transform.GetGrid(args.Target) is not { } grid || !TryComp(grid, out MapGridComponent? mapGrid))
                return;

            var tileDef = _tileDef[tileId];
            var tile = new Tile(tileDef.TileId);

            _mapSystem.SetTile(grid, mapGrid, args.Target, tile);
            Spawn(CultTileSpawnEffect, args.Target);
        }

        if (args.Audio is { } audio)
            _audio.PlayPvs(audio, args.Target);

        args.Handled = true;
    }

    private void OnPhaseShift(Goobstation.Shared.BloodCult.Spells.PhaseShiftEvent args)
    {
        if (args.Handled)
            return;

        if (_statusEffects.TryAddStatusEffect<PhaseShiftedComponent>(
            args.Performer,
            args.StatusEffectId,
            args.Duration,
            false))
            args.Handled = true;
    }
}
