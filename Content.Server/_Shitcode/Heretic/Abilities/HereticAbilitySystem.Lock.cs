// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.GameTicking.Rules;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Actions.Components;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.Heretic;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server.Heretic.Abilities;

public sealed partial class HereticAbilitySystem
{
    private void SubscribeLock()
    {
        SubscribeLocalEvent<HereticComponent, EventHereticBulglarFinesse>(OnBulglarFinesse);
        SubscribeLocalEvent<HereticComponent, EventHereticLastRefugee>(OnLastRefugee);
        // add eldritch id here

        SubscribeLocalEvent<HereticComponent, HereticAscensionLockEvent>(OnAscensionLock);

        SubscribeLocalEvent<GhoulComponent, EventHereticShapeshift>(OnShapeshift);

        SubscribeLocalEvent<ShapeshiftActionComponent, HereticShapeshiftMessage>(OnShapeshiftMessage);
    }

    private void OnShapeshiftMessage(Entity<ShapeshiftActionComponent> ent, ref HereticShapeshiftMessage args)
    {
        var key = args.UiKey;
        var user = args.Actor;

        if (!TryComp(user, out ActorComponent? actor))
            return;

        var session = actor.PlayerSession;

        _ui.CloseUi(ent.Owner, key);

        if (!HasComp<GhoulComponent>(user))
            return;

        if (!TryComp(ent, out ActionComponent? action) || !_actions.ValidAction((ent, action)))
            return;

        // We have to do this shit because otherwise actor isn't removed from client ui actors list and ui remains
        // opened after polymorph
        _pvs.AddSessionOverride(user, session);

        var polymorphed = _poly.PolymorphEntity(user, args.ProtoId);

        _actions.StartUseDelay((ent, action));

        if (polymorphed == null)
            return;

        // This shouldn't break because ghoul comp should be copied on polymorph (it copies max health),
        // change this behavior if this ability is ever given to heretic
        if (TryComp(user, out DamageableComponent? userDamage) &&
            TryComp(polymorphed.Value, out DamageableComponent? polymorphedDamage))
            _dmg.SetDamage(polymorphed.Value, polymorphedDamage, userDamage.Damage);

        _npcFaction.AddFaction(polymorphed.Value, HereticRuleSystem.HereticFactionId);

        if (TryComp(polymorphed, out GhoulComponent? ghoul))
            ghoul.ExamineMessage = null;

        var speech = Loc.GetString(ent.Comp.Speech);

        // Spawning a timer because otherwise speech wouldn't trigger (same issue as wizard polymorphs)
        Timer.Spawn(200,
            () =>
            {
                if (!Timing.InSimulation)
                    return;

                _pvs.RemoveSessionOverride(user, session);

                if (TerminatingOrDeleted(polymorphed.Value))
                    return;

                _chat.TrySendInGameICMessage(polymorphed.Value, speech, InGameICChatType.Speak, false);
            });
    }

    private void OnShapeshift(Entity<GhoulComponent> ent, ref EventHereticShapeshift args)
    {
        if (args.Handled || !HasComp<ShapeshiftActionComponent>(args.Action))
            return;

        _ui.TryOpenUi(args.Action.Owner, HereticShapeshiftUiKey.Key, ent);
    }

    private void OnBulglarFinesse(Entity<HereticComponent> ent, ref EventHereticBulglarFinesse args)
    {

    }
    private void OnLastRefugee(Entity<HereticComponent> ent, ref EventHereticLastRefugee args)
    {

    }

    private void OnAscensionLock(Entity<HereticComponent> ent, ref HereticAscensionLockEvent args)
    {

    }
}
