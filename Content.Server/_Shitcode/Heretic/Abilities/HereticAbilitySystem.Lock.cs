// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.GameTicking.Rules;
using Content.Server.Heretic.EntitySystems;
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

        _npcFaction.AddFaction(polymorphed.Value, HereticSystem.HereticFactionId);

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
}
