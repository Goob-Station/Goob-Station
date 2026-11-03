using System.Linq;
using Content.Goobstation.Shared.Wizard.FistFight;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mind.Components;
using Robust.Shared.Map;

namespace Content.Goobstation.Server.Wizard.FistFight;

public sealed partial class WizardFistFightSystem
{
    private void ReturnEveryone(WizardFistFightRuleComponent comp)
    {
        foreach (var participant in new[] { comp.Caster, comp.Challenger })
        {
            if (participant is not { } uid || !Exists(uid))
                continue;

            if (TryComp<WizardFistFighterComponent>(uid, out var fighter))
                RestoreFighter(comp, (uid, fighter));

            if (comp.Home is { } home)
                TeleportHome(comp, uid, home);
        }

        var now = Timing.CurTime;
        var query = EntityQueryEnumerator<WizardFistFightSpectatorComponent>();
        while (query.MoveNext(out var uid, out var spectator))
        {
            RemCompDeferred<WizardFistFightSpectatorComponent>(uid);
            if (!spectator.WasPacified)
                RemComp<PacifiedComponent>(uid);
            if (spectator.PacifiedUntil is { } until && until > now)
                _status.TryAddStatusEffect(uid, PacifiedStatus, until - now, true, PacifiedStatus);
            if (!spectator.WasGodmoded)
                _godmode.DisableGodmode(uid);

            TeleportHome(comp, uid, spectator.Home);
        }

        DumpLeftovers(comp);

        if (!comp.PrizePaid && comp.Winner is { } winner && Exists(winner))
        {
            comp.PrizePaid = true;
            RewardWinner(comp, winner);
        }
    }

    private void DumpLeftovers(WizardFistFightRuleComponent comp)
    {
        EntityCoordinates? destination = null;
        if (comp.Home is { } home)
            destination = IsUsable(comp, home) ? home : RandomStationSpawn(comp);
        destination ??= RandomStationSpawn(comp);

        if (destination is not { } spot)
        {
            Log.Warning("Wizard fist fight could not find anywhere on the station to dump the leftovers.");
            return;
        }

        foreach (var uid in comp.Remains)
        {
            if (!Exists(uid) || Transform(uid).MapID != comp.ArenaMap || _container.IsEntityInContainer(uid))
                continue;

            _transform.SetCoordinates(uid, spot.Offset(Scatter()));
            _transform.AttachToGridOrMap(uid);
        }

        comp.Remains.Clear();

        if (comp.Storage is not { } storage)
            return;

        foreach (var item in storage.ContainedEntities.ToList())
            _container.Remove(item, storage, destination: spot.Offset(Scatter()));
    }

    /// <summary>
    /// There shouldn't really be any way to still exist in the arena after the event is over but just in case
    /// we teleport them to the station intead of atomizing them with the map.
    /// </summary>
    private void EvacuateArena(WizardFistFightRuleComponent comp)
    {
        if (comp.ArenaMap is not { } arena)
            return;

        var query = EntityQueryEnumerator<MindContainerComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var mind, out var xform))
        {
            if (!mind.HasMind || xform.MapID != arena)
                continue;

            if (RandomStationSpawn(comp) is { } spawn)
            {
                Teleport(uid, spawn);
                continue;
            }

            _container.TryRemoveFromContainer(uid, true);
            _transform.DetachEntity(uid, xform);
            Log.Warning($"Wizard fist fight could not find anywhere on the station to evacuate {ToPrettyString(uid)} to. They were sent to nullspace.");
        }
    }

    private void RestoreFighter(WizardFistFightRuleComponent comp, Entity<WizardFistFighterComponent> ent)
    {
        var (uid, fighter) = ent;

        RemComp<WizardFistFighterComponent>(uid);

        if (fighter.Gloves is { } gloves && Exists(gloves))
        {
            _inventory.TryUnequip(uid, GlovesSlot, out _, silent: true, force: true);
            QueueDel(gloves);
        }

        if (fighter.OriginalGloves is { } original && Exists(original)
            && !_inventory.TryEquip(uid, original, GlovesSlot, silent: true, force: true))
            GiveOrDrop(comp, uid, original);

        foreach (var item in fighter.StoredItems)
            if (Exists(item))
                GiveOrDrop(comp, uid, item);

        _rejuvenate.PerformRejuvenate(uid);

        var now = Timing.CurTime;
        if (fighter.PacifiedUntil is { } until && until > now)
            _status.TryAddStatusEffect(uid, PacifiedStatus, until - now, true, PacifiedStatus);
        else if (fighter.WasPacified)
            EnsureComp<PacifiedComponent>(uid);
    }

    private void GiveOrDrop(WizardFistFightRuleComponent comp, EntityUid owner, EntityUid item)
    {
        if (_hands.TryForcePickupAnyHand(owner, item))
            return;

        _container.Remove(item, comp.Storage!, destination: Transform(owner).Coordinates);
    }

    protected override void Ended(EntityUid uid, WizardFistFightRuleComponent comp, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, comp, gameRule, args);

        if (comp.Spawner is { } spawner)
            QueueDel(spawner);

        ReturnEveryone(comp);
        EvacuateArena(comp);

        if (comp.MusicEntity is { } music)
            _audio.Stop(music);

        if (comp.ArenaMap is { } map && _map.MapExists(map))
            _map.DeleteMap(map);
    }
}
