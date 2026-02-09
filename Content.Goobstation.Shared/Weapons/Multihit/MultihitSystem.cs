// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Numerics;
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

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MultihitComponent, MeleeHitEvent>(OnHit);

        SubscribeLocalEvent<HereticComponent, MultihitUserHereticEvent>(HereticCheck);
        SubscribeLocalEvent<MultihitUserWhitelistEvent>(WhitelistCheck);

        SubscribeNetworkEvent<ResetMultihitLastAttackEvent>(OnReset);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;

        var query = EntityQueryEnumerator<ActiveMultihitComponent, MeleeWeaponComponent>();
        while (query.MoveNext(out var uid, out var active, out var melee))
        {
            if (active.LastAttack == null && active.QueuedAttacks.Count == 0)
            {
                RemCompDeferred(uid, active);
                continue;
            }

            if (curTime < (active.LastAttack ?? active.QueuedAttacks.Peek()).AttackTime)
                continue;

            if (ResolveUser((uid, active)) is not { } user)
            {
                RemCompDeferred(uid, active);
                continue;
            }

            QueuedMultihitAttack attack;

            // This is an endless war with prediction
            if (_net.IsClient)
            {
                active.LastAttack ??= active.QueuedAttacks.Dequeue();
                attack = active.LastAttack;
            }
            else
            {
                active.LastAttack = null;
                attack = active.QueuedAttacks.Dequeue();
                RaiseNetworkEvent(new ResetMultihitLastAttackEvent(GetNetEntity(uid)), user);
            }

            PerformFollowupAttack((uid, active, melee), attack, user);
        }
    }

    private void OnReset(ResetMultihitLastAttackEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession != _player.LocalSession || !TryGetEntity(msg.Weapon, out var weapon) ||
            !TryComp(weapon.Value, out ActiveMultihitComponent? active))
            return;

        active.LastAttack = null;
    }

    private EntityUid? ResolveUser(Entity<ActiveMultihitComponent> ent)
    {
        if (ent.Comp.User == null || TerminatingOrDeleted(ent.Comp.User.Value) ||
            !_hands.IsHolding(ent.Comp.User.Value, ent))
            return null;

        return ent.Comp.User.Value;
    }

    private void PerformFollowupAttack(Entity<ActiveMultihitComponent, MeleeWeaponComponent> ent,
        QueuedMultihitAttack attack,
        EntityUid user)
    {
        var (uid, active, melee) = ent;

        active.NextDamageMultiplier = attack.DamageMultiplier;

        if (attack.Direction != null)
            TryPerformMultihitHeavyAttack((uid, melee), attack.Direction.Value, user);
        else if (TryGetEntity(attack.Target, out var target))
            TryPerformMultihitLightAttack((uid, melee), target.Value, user);

        active.NextDamageMultiplier = 1f;
    }

    private bool TryPerformMultihitLightAttack(Entity<MeleeWeaponComponent> ent, EntityUid target, EntityUid user)
    {
        var (uid, weapon) = ent;

        if (TerminatingOrDeleted(target))
            return false;

        var inCombat = _combatMode.IsInCombatMode(user);
        if (!inCombat)
            _combatMode.SetInCombatMode(user, true);
        var result = _melee.AttemptLightAttack(user, uid, weapon, target);
        if (!inCombat)
            _combatMode.SetInCombatMode(user, false);
        return result;
    }

    private bool TryPerformMultihitHeavyAttack(Entity<MeleeWeaponComponent> ent, Vector2 direction, EntityUid user)
    {
        var (uid, weapon) = ent;

        var xform = Transform(user);
        var userCoords = _transform.GetMapCoordinates(user, xform);
        var distance = MathF.Min(weapon.Range, direction.Length());
        var angle = direction.ToWorldAngle();
        var entities = _melee.ArcRayCast(userCoords.Position,
                angle,
                weapon.Angle,
                distance,
                xform.MapID,
                user)
            .ToList();

        var inCombat = _combatMode.IsInCombatMode(user);
        if (!inCombat)
            _combatMode.SetInCombatMode(user, true);
        var result = _melee.AttemptHeavyAttack(user,
            uid,
            weapon,
            entities,
            _transform.ToCoordinates(userCoords.Offset(direction)));
        if (!inCombat)
            _combatMode.SetInCombatMode(user, false);
        return result;
    }

    private void WhitelistCheck(MultihitUserWhitelistEvent ev)
    {
        ev.Handled = ev.Blacklist
            ? _whitelist.IsBlacklistFail(ev.Whitelist, ev.User)
            : _whitelist.IsWhitelistPass(ev.Whitelist, ev.User);
    }

    private void HereticCheck(Entity<HereticComponent> ent, ref MultihitUserHereticEvent args)
    {
        args.Handled = (args.RequiredPath == null || ent.Comp.CurrentPath == args.RequiredPath) &&
                       ent.Comp.PathStage >= args.MinPathStage;
    }

    private void OnHit(EntityUid uid, MultihitComponent component, MeleeHitEvent args)
    {
        if (_net.IsClient && _player.LocalEntity != args.User)
            return;

        if (!args.IsHit || args.Weapon == args.User)
            return;

        if (args.Direction == null)
        {
            if (args.HitEntities.Count == 0)
                return;

            if (args.HitEntities[0] == args.User)
                return;
        }

        if (HasComp<ActiveMultihitComponent>(uid))
            return;

        if (!CheckConditions())
            return;

        var curTime = _timing.CurTime;
        var delay = component.MultihitDelay;
        var penalty = TimeSpan.Zero;

        foreach (var held in _hands.EnumerateHeld(args.User))
        {
            if (!TryMultihitAttack(held))
                continue;

            delay += component.MultihitDelay;
            penalty += component.DelayPenalty;
        }

        if (penalty <= TimeSpan.Zero || !TryComp(uid, out MeleeWeaponComponent? melee))
            return;

        melee.NextAttack += penalty;

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

        bool TryMultihitAttack(EntityUid weapon)
        {
            if (weapon == uid)
                return false;

            if (component.MultihitWhitelist != null && !_whitelist.IsValid(component.MultihitWhitelist, weapon))
                return false;

            if (!TryComp(weapon, out MeleeWeaponComponent? melee))
                return false;

            var active = EnsureComp<ActiveMultihitComponent>(weapon);

            if (active.User != null && active.User != args.User)
                active.QueuedAttacks.Clear();

            active.User = args.User;

            var attack = new QueuedMultihitAttack
            {
                AttackTime = curTime + delay,
                DamageMultiplier = component.DamageMultiplier,
                Target = args.HitEntities.Count == 0 ? null : GetNetEntity(args.HitEntities[0]),
                Direction = args.Direction,
            };

            active.QueuedAttacks.Enqueue(attack);
            active.LastAttack = null;
            active.DeletionAccumulator = 0f;
            Dirty(weapon, active);
            return true;
        }
    }
}
