// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared._Shitcode.Heretic.Systems;
using Content.Shared.CombatMode;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Heretic;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Whitelist;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Weapons.Multihit;

public sealed class MultihitSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedMeleeWeaponSystem _melee = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedCombatModeSystem _combatMode = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedHereticSystem _heretic = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MeleeHitEvent>(OnHit, before: new[] { typeof(ActiveMultihitSystem) });

        SubscribeLocalEvent<MultihitUserHereticEvent>(HereticCheck);
        SubscribeLocalEvent<MultihitUserWhitelistEvent>(WhitelistCheck);
    }

    private void WhitelistCheck(MultihitUserWhitelistEvent ev)
    {
        ev.Handled = ev.Blacklist
            ? _whitelist.IsWhitelistFail(ev.Whitelist, ev.User)
            : _whitelist.IsWhitelistPass(ev.Whitelist, ev.User);
    }

    private void HereticCheck(MultihitUserHereticEvent args)
    {
        if (!_heretic.TryGetHereticComponent(args.User, out var heretic, out _))
            return;

        args.Handled = (args.RequiredPath == null || heretic.CurrentPath == args.RequiredPath) &&
                       heretic.PathStage >= args.MinPathStage;
    }

    private void OnHit(MeleeHitEvent args)
    {
        if (_net.IsClient && _player.LocalEntity != args.User)
            return;

        if (!_timing.IsFirstTimePredicted || !args.IsHit)
            return;

        if (args.Direction == null)
        {
            if (args.HitEntities.Count == 0)
                return;

            if (args.HitEntities[0] == args.User)
                return;
        }

        if (HasComp<ActiveMultihitComponent>(args.Weapon))
            return;

        if (TryComp<MultihitComponent>(args.Weapon, out var weaponMultihit))
            DoMultihit(args.Weapon, weaponMultihit, args);

        if (args.Weapon != args.User && TryComp<MultihitComponent>(args.User, out var userMultihit))
            DoMultihit(args.User, userMultihit, args);
    }

    private void DoMultihit(EntityUid uid, MultihitComponent component, MeleeHitEvent args)
    {
        if (!CheckConditions())
            return;

        var delay = component.MultihitDelay;

        if (uid == args.User)
        {
            var gather = new MultihitGetWeaponsEvent(args.User, args.Weapon, component.DamageMultiplier, component.MultihitDelay);
            RaiseLocalEvent(args.User, ref gather);

            delay = gather.Delay;
            foreach (var weapon in gather.Weapons)
                if (TryMultihitAttack(weapon, gather.DamageMultiplier, requireHeld: false))
                    delay += gather.Delay;
        }
        else
            foreach (var held in _hands.EnumerateHeld(args.User))
                if (TryMultihitAttack(held, component.DamageMultiplier, requireHeld: true))
                    delay += component.MultihitDelay;

        return;

        bool CheckConditions()
        {
            if (component.Conditions.Count == 0)
                return true;

            foreach (var ev in component.Conditions)
            {
                ev.Handled = false;
                ev.User = args.User;
                RaiseLocalEvent(args.User, (object) ev, true);
                switch (ev.Handled)
                {
                    case false when component.RequireAllConditions:
                        return false;
                    case true when !component.RequireAllConditions:
                        return true;
                }
            }

            return component.RequireAllConditions;
        }

        bool StillValid(EntityUid weapon, bool requireHeld)
        {
            if (requireHeld)
                return _hands.IsHolding(args.User, weapon);

            var gather = new MultihitGetWeaponsEvent(args.User, args.Weapon, component.DamageMultiplier, component.MultihitDelay);
            RaiseLocalEvent(args.User, ref gather);
            return gather.Weapons.Contains(weapon);
        }

        bool TryMultihitAttack(EntityUid weapon, float damageMultiplier, bool requireHeld)
        {
            if (weapon == args.Weapon)
                return false;

            if (component.MultihitWhitelist != null && !_whitelist.IsValid(component.MultihitWhitelist, weapon))
                return false;

            if (!TryComp(weapon, out MeleeWeaponComponent? melee))
                return false;

            EnsureComp<ActiveMultihitComponent>(weapon).DamageMultiplier *= damageMultiplier;

            if (args.Direction == null)
            {
                Timer.Spawn(delay,
                    () =>
                    {
                        if (TerminatingOrDeleted(weapon) ||
                            !TryComp(weapon, out ActiveMultihitComponent? activeMultihit))
                            return;

                        var target = args.HitEntities[0];

                        if (TerminatingOrDeleted(args.User)
                        || TerminatingOrDeleted(target)
                        || !Resolve(weapon, ref melee, false)
                        || !StillValid(weapon, requireHeld))
                        {
                            RemComp(weapon, activeMultihit);
                            return;
                        }

                        var inCombat = _combatMode.IsInCombatMode(args.User);
                        if (!inCombat)
                            _combatMode.SetInCombatMode(args.User, true);
                        _melee.AttemptLightAttack(args.User, weapon, melee, target);
                        if (!inCombat)
                            _combatMode.SetInCombatMode(args.User, false);

                        if (Resolve(weapon, ref activeMultihit, false))
                            RemComp(weapon, activeMultihit);
                    });
            }
            else
            {
                Timer.Spawn(delay,
                    () =>
                    {
                        if (TerminatingOrDeleted(weapon) ||
                            !TryComp(weapon, out ActiveMultihitComponent? activeMultihit))
                            return;

                        if (TerminatingOrDeleted(args.User)
                        || TerminatingOrDeleted(weapon)
                        || !TryComp(args.User, out TransformComponent? xform)
                        || !Resolve(weapon, ref melee, false)
                        || !StillValid(weapon, requireHeld))
                        {
                            RemComp(weapon, activeMultihit);
                            return;
                        }

                        var userCoords = _transform.GetMapCoordinates(args.User, xform);
                        var distance = MathF.Min(melee.Range, args.Direction.Value.Length());
                        var angle = args.Direction.Value.ToWorldAngle();
                        var entities = _melee.ArcRayCast(userCoords.Position,
                                angle,
                                melee.Angle,
                                distance,
                                xform.MapID,
                                args.User)
                            .ToList();

                        var inCombat = _combatMode.IsInCombatMode(args.User);
                        if (!inCombat)
                            _combatMode.SetInCombatMode(args.User, true);
                        _melee.AttemptHeavyAttack(args.User,
                            weapon,
                            melee,
                            entities,
                            _transform.ToCoordinates(userCoords.Offset(args.Direction.Value)));
                        if (!inCombat)
                            _combatMode.SetInCombatMode(args.User, false);

                        if (Resolve(weapon, ref activeMultihit, false))
                            RemComp(weapon, activeMultihit);
                    });
            }

            return true;
        }
    }
}
