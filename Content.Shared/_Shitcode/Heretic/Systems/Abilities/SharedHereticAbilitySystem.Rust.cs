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
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Map;

namespace Content.Shared._Shitcode.Heretic.Systems.Abilities;

public abstract partial class SharedHereticAbilitySystem
{
    public const string RustTile = "PlatingRust";

    protected virtual void SubscribeRust()
    {
        SubscribeLocalEvent<RustbringerComponent, BeforeStaminaDamageEvent>(OnBeforeStaminaDamage);
        SubscribeLocalEvent<RustbringerComponent, BeforeStatusEffectAddedEvent>(OnBeforeStatusEffect);
        SubscribeLocalEvent<RustbringerComponent, SlipAttemptEvent>(OnSlipAttempt);
        SubscribeLocalEvent<RustbringerComponent, GetExplosionResistanceEvent>(OnGetExplosionResists);
        SubscribeLocalEvent<RustbringerComponent, ElectrocutionAttemptEvent>(OnElectrocuteAttempt);
        SubscribeLocalEvent<RustbringerComponent, BeforeHarmfulActionEvent>(OnBeforeHarmfulAction);
        SubscribeLocalEvent<RustbringerComponent, DamageModifyEvent>(OnModifyDamage);
    }

    private void OnModifyDamage(Entity<RustbringerComponent> ent, ref DamageModifyEvent args)
    {
        if (!IsTileRust(Transform(ent).Coordinates, out _))
            return;

        args.Damage = DamageSpecifier.ApplyModifierSet(args.Damage, ent.Comp.ModifierSet);
    }

    private void OnBeforeHarmfulAction(Entity<RustbringerComponent> ent, ref BeforeHarmfulActionEvent args)
    {
        if (args.Cancelled || args.Type == HarmfulActionType.Harm)
            return;

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
