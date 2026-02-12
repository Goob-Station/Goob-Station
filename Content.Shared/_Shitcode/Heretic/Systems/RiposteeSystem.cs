// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.MartialArts;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Systems.Abilities;
using Content.Shared.ActionBlocker;
using Content.Shared.CombatMode;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Standing;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Heretic.Systems;

public sealed class RiposteeSystem : EntitySystem
{
    [Dependency] private readonly SharedMeleeWeaponSystem _melee = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedCombatModeSystem _combatMode = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RiposteeComponent, BeforeHarmfulActionEvent>(OnHarmAttempt,
            before: new[] { typeof(SharedHereticAbilitySystem) });

        SubscribeNetworkEvent<RiposteUsedEvent>(OnRiposteUsed);
    }

    private void OnRiposteUsed(RiposteUsedEvent ev)
    {
        if (_net.IsServer)
            return;

        if (!TryGetEntity(ev.User, out var user) || !TryGetEntity(ev.Target, out var target) ||
            !TryGetEntity(ev.Weapon, out var weapon))
            return;

        if (_player.LocalEntity != user.Value)
            return;

        if (!TryComp(weapon.Value, out MeleeWeaponComponent? melee) ||
            !TryComp(user.Value, out RiposteeComponent? ripostee))
            return;

        CounterAttack((weapon.Value, melee), (user.Value, ripostee), target.Value, ev.Data);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_net.IsClient)
            return;

        var eqe = EntityQueryEnumerator<RiposteeComponent>();
        while (eqe.MoveNext(out var uid, out var rip))
        {
            if (!uid.IsValid())
                continue;

            foreach (var data in rip.Data.Values)
            {
                if (data.Cooldown <= 0f)
                {
                    data.CanRiposte = true;
                    continue;
                }

                if (data.CanRiposte)
                    continue;

                data.Timer -= frameTime;

                if (data.Timer > 0)
                    continue;

                data.Timer = data.Cooldown;

                data.CanRiposte = true;
                if (data.RiposteAvailableMessage != null)
                    _popup.PopupEntity(Loc.GetString(data.RiposteAvailableMessage), uid, uid);
            }
        }
    }

    private void OnHarmAttempt(Entity<RiposteeComponent> ent, ref BeforeHarmfulActionEvent args)
    {
        if (args.Cancelled)
            return;

        if (_net.IsClient)
            return;

        if (_mobState.IsIncapacitated(ent))
            return;

        if (HasComp<RiposteeComponent>(args.User))
            return;

        foreach (var data in ent.Comp.Data.Values)
        {
            if (!data.CanRiposte)
                continue;

            if (args.User == ent.Owner)
                continue;

            if (data.CanRiposteEvent != null)
            {
                var ev = data.CanRiposteEvent;
                ev.Handled = false;
                RaiseLocalEvent(ent, (object) ev);
                if (!ev.Handled)
                    continue;
            }

            if (!data.CanRiposteWhileProne && _standing.IsDown(ent))
                continue;

            if (data.RiposteChance is > 0f and < 1f)
            {
                if (!_random.Prob(data.RiposteChance))
                    continue;
            }

            Entity<MeleeWeaponComponent>? weapon = null;
            if (data.RequiresWeapon)
            {
                foreach (var held in _hands.EnumerateHeld(ent.Owner))
                {
                    if (_whitelist.IsWhitelistPassOrNull(data.WeaponWhitelist, held) &&
                        TryComp(held, out MeleeWeaponComponent? melee))
                        weapon = (held, melee);
                }
            }
            else
            {
                if (TryComp(ent.Owner, out MeleeWeaponComponent? melee)
                    && _hands.TryGetEmptyHand(ent.Owner, out _))
                    weapon = (ent.Owner, melee);
            }

            if (weapon == null)
                continue;

            if (!_blocker.CanAttack(ent, args.User, weapon.Value))
                continue;

            args.Cancel();

            if (data.Cooldown > 0f)
                data.CanRiposte = false;

            CounterAttack(weapon.Value, ent, args.User, data);
            RaiseNetworkEvent(new RiposteUsedEvent(GetNetEntity(ent.Owner),
                    GetNetEntity(args.User),
                    GetNetEntity(weapon.Value.Owner),
                    data),
                ent.Owner);

            break;
        }
    }

    private void CounterAttack(Entity<MeleeWeaponComponent> weapon,
        Entity<RiposteeComponent> user,
        EntityUid target,
        RiposteData data)
    {
        var nextAttack = weapon.Comp.NextAttack;
        weapon.Comp.NextAttack = TimeSpan.Zero;

        RaiseLocalEvent(user, new SaveLastAttacksEvent());
        RaiseLocalEvent(user, new ResetLastAttacksEvent(false));

        var inCombat = _combatMode.IsInCombatMode(user);
        if (!inCombat)
            _combatMode.SetInCombatMode(user, true);

        if (_melee.AttemptLightAttack(user, weapon.Owner, weapon.Comp, target) && _net.IsServer &&
            _melee.InRange(user, target, weapon.Comp.Range, CompOrNull<ActorComponent>(user)?.PlayerSession))
        {
            if (data.StunTime > TimeSpan.Zero)
                _stun.TryStun(target, data.StunTime, true);

            if (data.KnockdownTime > TimeSpan.Zero)
                _stun.TryKnockdown(target, data.KnockdownTime, true);
        }

        if (!inCombat)
            _combatMode.SetInCombatMode(user, false);

        RaiseLocalEvent(user, new LoadLastAttacksEvent());

        weapon.Comp.NextAttack = nextAttack;
        Dirty(weapon);

        if (_net.IsClient && _player.LocalEntity == target)
            return;

        _audio.PlayPredicted(data.RiposteSound, user, user);

        if (data.RiposteUsedMessage != null)
            _popup.PopupClient(Loc.GetString(data.RiposteUsedMessage), user, user);
    }
}

[Serializable, NetSerializable]
public sealed class RiposteUsedEvent(NetEntity user, NetEntity target, NetEntity weapon, RiposteData data)
    : EntityEventArgs
{
    public NetEntity User = user;

    public NetEntity Target = target;

    public NetEntity Weapon = weapon;

    public RiposteData Data = data;
}
