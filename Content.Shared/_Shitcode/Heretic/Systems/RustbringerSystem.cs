using System.Diagnostics.CodeAnalysis;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared.Actions.Events;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Electrocution;
using Content.Shared.Explosion;
using Content.Shared.Maps;
using Content.Shared.Slippery;
using Content.Shared.StatusEffect;
using Robust.Shared.Map;

namespace Content.Shared._Goobstation.Heretic.Systems;

public sealed class RustbringerSystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapMan = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefinitionManager = default!;

    public const string RustTile = "PlatingRust";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RustbringerComponent, BeforeStaminaDamageEvent>(OnBeforeStaminaDamage);
        SubscribeLocalEvent<RustbringerComponent, BeforeStatusEffectAddedEvent>(OnBeforeStatusEffect);
        SubscribeLocalEvent<RustbringerComponent, SlipAttemptEvent>(OnSlipAttempt);
        SubscribeLocalEvent<RustbringerComponent, GetExplosionResistanceEvent>(OnGetExplosionResists);
        SubscribeLocalEvent<RustbringerComponent, ElectrocutionAttemptEvent>(OnElectrocuteAttempt);
        SubscribeLocalEvent<RustbringerComponent, DisarmAttemptEvent>(OnDisarmAttempt);
        SubscribeLocalEvent<RustbringerComponent, DamageModifyEvent>(OnModifyDamage);
    }

    private void OnModifyDamage(Entity<RustbringerComponent> ent, ref DamageModifyEvent args)
    {
        if (!IsTileRust(Transform(ent).Coordinates, out _))
            return;

        var specifier = new DamageModifierSet
        {
            Coefficients =
            {
                {"Caustic", 0f},
                {"Poison", 0f},
                {"Radiation", 0f},
                {"Cellular", 0f},
            },
        };

        args.Damage = DamageSpecifier.ApplyModifierSet(args.Damage, specifier);
    }

    private void OnDisarmAttempt(Entity<RustbringerComponent> ent, ref DisarmAttemptEvent args)
    {
        if (!IsTileRust(Transform(ent).Coordinates, out _))
            return;

        args.Cancel();
    }

    private void OnElectrocuteAttempt(Entity<RustbringerComponent> ent, ref ElectrocutionAttemptEvent args)
    {
        if (!IsTileRust(Transform(ent).Coordinates, out _))
            return;

        args.Cancel();
    }

    private void OnGetExplosionResists(Entity<RustbringerComponent> ent, ref GetExplosionResistanceEvent args)
    {
        if (!IsTileRust(Transform(ent).Coordinates, out _))
            return;

        args.DamageCoefficient *= 0f;
    }

    private void OnSlipAttempt(Entity<RustbringerComponent> ent, ref SlipAttemptEvent args)
    {
        if (!IsTileRust(Transform(ent).Coordinates, out _))
            return;

        args.NoSlip = true;
    }

    private void OnBeforeStatusEffect(Entity<RustbringerComponent> ent, ref BeforeStatusEffectAddedEvent args)
    {
        if (!IsTileRust(Transform(ent).Coordinates, out _))
            return;

        if (args.Key is not ("KnockedDown" or "Stun"))
            return;

        args.Cancelled = true;
    }

    private void OnBeforeStaminaDamage(Entity<RustbringerComponent> ent, ref BeforeStaminaDamageEvent args)
    {
        if (!IsTileRust(Transform(ent).Coordinates, out _))
            return;

        args.Cancelled = true;
    }

    public bool IsTileRust(EntityCoordinates coords, [NotNullWhen(true)] out Vector2i? tileCoords)
    {
        tileCoords = null;
        if (!_mapMan.TryFindGridAt(_transform.ToMapCoordinates(coords), out var gridUid, out var mapGrid))
            return false;

        var tileRef = _map.GetTileRef(gridUid, mapGrid, coords);
        var tileDef = (ContentTileDefinition) _tileDefinitionManager[tileRef.Tile.TypeId];

        tileCoords = tileRef.GridIndices;
        return tileDef.ID == RustTile;
    }
}
