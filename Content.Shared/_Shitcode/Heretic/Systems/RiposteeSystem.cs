// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._EinsteinEngines.TelescopicBaton;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Systems.Abilities;
using Content.Shared.ActionBlocker;
using Content.Shared.CombatMode;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;
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
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;

    private static readonly SoundSpecifier RiposteSound =
        new SoundPathSpecifier("/Audio/_Goobstation/Heretic/parry.ogg");

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

        if (!TryComp(weapon.Value, out MeleeWeaponComponent? melee))
            return;

        CounterAttack((weapon.Value, melee), user.Value, target.Value);
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

            if (rip.CanRiposte)
                continue;

            rip.Timer -= frameTime;

            if (rip.Timer <= 0)
            {
                rip.Timer = rip.Cooldown;

                rip.CanRiposte = true;
                _popup.PopupEntity(Loc.GetString("heretic-riposte-available"), uid, uid);
            }
        }
    }

    private void OnHarmAttempt(Entity<RiposteeComponent> ent, ref BeforeHarmfulActionEvent args)
    {
        if (args.Cancelled)
            return;

        if (!ent.Comp.CanRiposte)
            return;

        if (_net.IsClient)
            return;

        if (args.User == ent.Owner)
            return;

        Entity<MeleeWeaponComponent>? weapon = null;
        foreach (var held in _hands.EnumerateHeld(ent.Owner))
        {
            if (_whitelist.IsWhitelistPassOrNull(ent.Comp.WeaponWhitelist, held) &&
                TryComp(held, out MeleeWeaponComponent? melee))
                weapon = (held, melee);
        }

        if (weapon == null)
            return;

        if (!_blocker.CanAttack(ent, args.User, weapon.Value))
            return;

        args.Cancel();

        CounterAttack(weapon.Value, ent, args.User);
        RaiseNetworkEvent(new RiposteUsedEvent(GetNetEntity(ent.Owner),
                GetNetEntity(args.User),
                GetNetEntity(weapon.Value.Owner)),
            ent.Owner);

        ent.Comp.CanRiposte = false;
    }

    private void CounterAttack(Entity<MeleeWeaponComponent> weapon, EntityUid user, EntityUid target)
    {
        var nextAttack = weapon.Comp.NextAttack;
        weapon.Comp.NextAttack = TimeSpan.Zero;
        var knockdown = HasComp<KnockdownOnHitComponent>(weapon);
        if (!knockdown)
            AddComp<KnockdownOnHitComponent>(weapon);
        var inCombat = _combatMode.IsInCombatMode(user);
        if (!inCombat)
            _combatMode.SetInCombatMode(user, true);
        _melee.AttemptLightAttack(user, weapon.Owner, weapon.Comp, target);
        if (!inCombat)
            _combatMode.SetInCombatMode(user, false);
        if (!knockdown)
            RemCompDeferred<KnockdownOnHitComponent>(weapon);
        weapon.Comp.NextAttack = nextAttack;
        Dirty(weapon);

        if (_net.IsClient && _player.LocalEntity == target)
            return;

        _audio.PlayPredicted(RiposteSound, user, user);
        _popup.PopupClient(Loc.GetString("heretic-riposte-used"), user, user);
    }
}

[Serializable, NetSerializable]
public sealed class RiposteUsedEvent(NetEntity user, NetEntity target, NetEntity weapon) : EntityEventArgs
{
    public NetEntity User = user;

    public NetEntity Target = target;

    public NetEntity Weapon = weapon;
}
