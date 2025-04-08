using Content.Shared.Atmos;
using Content.Shared.Damage;
using Content.Shared.Heretic;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Temperature.Components;
using Content.Server.Atmos.Components;
using Content.Server.Body.Components;
using Content.Server.Temperature.Components;
using Robust.Shared.Map.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using System.Linq;
using System.Threading.Tasks;

namespace Content.Server.Heretic.Abilities;

public sealed partial class HereticAbilitySystem : EntitySystem
{
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly TransformSystem _xform = default!;

    private void SubscribeAsh()
    {
        SubscribeLocalEvent<HereticComponent, EventHereticAshenShift>(OnJaunt);
        SubscribeLocalEvent<GhoulComponent, EventHereticAshenShift>(OnJauntGhoul);

        SubscribeLocalEvent<HereticComponent, EventHereticVolcanoBlast>(OnVolcano);
        SubscribeLocalEvent<HereticComponent, EventHereticNightwatcherRebirth>(OnNWRebirth);
        SubscribeLocalEvent<HereticComponent, EventHereticFlames>(OnFlames);
        SubscribeLocalEvent<HereticComponent, EventHereticCascade>(OnCascade);

        SubscribeLocalEvent<HereticComponent, HereticAscensionAshEvent>(OnAscensionAsh);
    }

    private void OnJaunt(Entity<HereticComponent> ent, ref EventHereticAshenShift args)
    {
        if (TryUseAbility(ent, args) && TryDoJaunt(ent))
            args.Handled = true;
    }
    private void OnJauntGhoul(Entity<GhoulComponent> ent, ref EventHereticAshenShift args)
    {
        if (TryUseAbility(ent, args) && TryDoJaunt(ent))
            args.Handled = true;
    }
    private bool TryDoJaunt(EntityUid ent)
    {
        Spawn("PolymorphAshJauntAnimation", Transform(ent).Coordinates);
        var urist = _poly.PolymorphEntity(ent, "AshJaunt");
        if (urist == null)
            return false;

        return true;
    }

    private void OnVolcano(Entity<HereticComponent> ent, ref EventHereticVolcanoBlast args)
    {
        if (!TryUseAbility(ent, args))
            return;

        var ignoredTargets = new List<EntityUid>();

        // all ghouls are immune to heretic shittery
        foreach (var e in EntityQuery<GhoulComponent>())
            ignoredTargets.Add(e.Owner);

        // all heretics with the same path are also immune
        foreach (var e in EntityQuery<HereticComponent>())
            if (e.CurrentPath == ent.Comp.CurrentPath)
                ignoredTargets.Add(e.Owner);

        if (!_splitball.Spawn(ent, ignoredTargets))
            return;

        if (ent.Comp is { Ascended: true, CurrentPath: "Ash" }) // will only work on ash path
            _flammable.AdjustFireStacks(ent, 20f, ignite: true);

        args.Handled = true;
    }
    private void OnNWRebirth(Entity<HereticComponent> ent, ref EventHereticNightwatcherRebirth args)
    {
        if (!TryUseAbility(ent, args))
            return;

        var power = ent.Comp.CurrentPath == "Ash" ? ent.Comp.PathStage : 4f;
        var lookup = _lookup.GetEntitiesInRange(ent, power);
        var healAmount = -10f - power;

        foreach (var look in lookup)
        {
            if ((TryComp<HereticComponent>(look, out var th) && th.CurrentPath == ent.Comp.CurrentPath)
            || HasComp<GhoulComponent>(look))
                continue;

            if (TryComp<FlammableComponent>(look, out var flam))
            {
                if (flam.OnFire && TryComp<DamageableComponent>(ent, out var dmgc))
                {
                    // heals everything by base + power for each burning target
                    _stam.TryTakeStamina(ent, healAmount);
                    var dmgdict = dmgc.Damage.DamageDict;
                    DamageSpecifier healSpecifier = new();

                    foreach (var key in dmgdict.Keys)
                    {
                        healSpecifier.DamageDict[key] = -dmgdict[key] < healAmount ? healAmount : -dmgdict[key];
                    }

                    _dmg.TryChangeDamage(ent, healSpecifier, true, false, dmgc);
                }

                if (flam.OnFire)
                    _flammable.AdjustFireStacks(look, power, flam, true);

                if (TryComp<MobStateComponent>(look, out var mobstat))
                    if (mobstat.CurrentState == MobState.Critical)
                        _mobstate.ChangeMobState(look, MobState.Dead, mobstat);
            }
        }

        args.Handled = true;
    }
    private void OnFlames(Entity<HereticComponent> ent, ref EventHereticFlames args)
    {
        if (!TryUseAbility(ent, args))
            return;

        EnsureComp<HereticFlamesComponent>(ent);

        if (ent.Comp.Ascended)
            _flammable.AdjustFireStacks(ent, 20f, ignite: true);

        args.Handled = true;
    }
    private void OnCascade(Entity<HereticComponent> ent, ref EventHereticCascade args)
    {
        if (!TryUseAbility(ent, args) || !Transform(ent).GridUid.HasValue)
            return;

        CombustArea(ent, 9, false);

        if (ent.Comp.Ascended)
            _flammable.AdjustFireStacks(ent, 20f, ignite: true);

        args.Handled = true;
    }


    private void OnAscensionAsh(Entity<HereticComponent> ent, ref HereticAscensionAshEvent args)
    {
        RemComp<TemperatureComponent>(ent);
        RemComp<TemperatureSpeedComponent>(ent);
        RemComp<RespiratorComponent>(ent);
        RemComp<BarotraumaComponent>(ent);

        // fire immunity
        var flam = EnsureComp<FlammableComponent>(ent);
        flam.Damage = new(); // reset damage dict
        // this does NOT protect you against lasers and whatnot. for now. when i figure out THIS STUPID FUCKING LIMB SYSTEM!!!
        // regards.
    }

    #region Helper methods

    [ValidatePrototypeId<EntityPrototype>] private static readonly EntProtoId FirePrototype = "HereticFireAA";

    public async Task CombustArea(EntityUid ent, int range = 1, bool hollow = true)
    {
        // we need this beacon in order for damage box to not break apart
        var beacon = Spawn(null, _xform.GetMapCoordinates((EntityUid) ent));

        for (int i = 0; i <= range; i++)
        {
            SpawnFireBox(beacon, range: i, hollow);
            await Task.Delay((int) 500f);
        }

        EntityManager.DeleteEntity(beacon); // cleanup
    }

    public void SpawnFireBox(EntityUid relative, int range = 0, bool hollow = true)
    {
        if (range == 0)
        {
            Spawn(FirePrototype, Transform(relative).Coordinates);
            return;
        }

        var xform = Transform(relative);

        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
            return;

        var gridEnt = ((EntityUid) xform.GridUid, grid);

        // get tile position of our entity
        if (!_xform.TryGetGridTilePosition(relative, out var tilePos))
            return;

        // make a box
        var pos = _map.TileCenterToVector(gridEnt, tilePos);
        var confines = new Box2(pos, pos).Enlarged(range);
        var box = _map.GetLocalTilesIntersecting(relative, grid, confines).ToList();

        // hollow it out if necessary
        if (hollow)
        {
            var confinesS = new Box2(pos, pos).Enlarged(Math.Max(range - 1, 0));
            var boxS = _map.GetLocalTilesIntersecting(relative, grid, confinesS).ToList();
            box = box.Where(b => !boxS.Contains(b)).ToList();
        }

        // fill the box
        foreach (var tile in box)
        {
            Spawn(FirePrototype, _map.GridTileToWorld((EntityUid) xform.GridUid, grid, tile.GridIndices));
        }
    }

    #endregion
}
