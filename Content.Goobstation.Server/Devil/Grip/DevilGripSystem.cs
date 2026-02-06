// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tim <timfalken@hotmail.com>
// SPDX-FileCopyrightText: 2025 Timfa <timfalken@hotmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Devil.Condemned;
using Content.Goobstation.Shared.Devil;
using Content.Goobstation.Shared.Devil.Components;
using Content.Goobstation.Shared.Devil.Condemned;
using Content.Goobstation.Shared.Religion;
using Content.Goobstation.Shared.Wraith.Curses;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Server.Chat.Systems;
using Content.Server.Speech.EntitySystems;
using Content.Shared.Actions;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Interaction;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Server.Devil.Grip;

public sealed class DevilGripSystem : EntitySystem
{
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly RatvarianLanguageSystem _language = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly DivineInterventionSystem _divineIntervention = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly CondemnedSystem _condemned = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DevilGripComponent, AfterInteractEvent>(OnAfterInteract);
    }

    private void OnAfterInteract(Entity<DevilGripComponent> ent, ref AfterInteractEvent args)
    {
        if (!args.CanReach
            || args.Target is not { } target
            || args.Target == args.User
            || _whitelist.IsBlacklistPass(ent.Comp.Blacklist, target)
            || !TryComp<DevilComponent>(args.User, out var devilComp))
            return;

        if (_divineIntervention.ShouldDeny(target))
        {
            CleanupGrip(args.User, devilComp, ent);
            args.Handled = true;
            return;
        }

        var stunTime = ent.Comp.KnockdownTime;

        // Upgrade: Enhanced stun
        if (HasComp<GripSidegradeStunComponent>(args.User))
        {
            stunTime += ent.Comp.KnockdownTimeIncrement;
            _damageable.TryChangeDamage(args.User, ent.Comp.Healing);
        }

        if (!TryComp(target, out StatusEffectsComponent? status))
            return;

        _stun.KnockdownOrStun(target, stunTime, true, status);
        _stamina.TakeStaminaDamage(target, ent.Comp.StaminaDamage);
        _language.DoRatvarian(target, ent.Comp.SpeechTime, true, status);

        // Upgrade: Rot curse
        if (HasComp<GripSidegradeRotComponent>(args.User))
        {
            var curseHolder = EnsureComp<CurseHolderComponent>(target);

            if (!curseHolder.ActiveCurses.Contains("CurseRot"))
            {
                var curseEv = new CurseAppliedEvent("CurseRot", args.User);
                RaiseLocalEvent(target, ref curseEv);
            }
        }

        // As archdevil, steals souls.
        if (HasComp<ArchdevilComponent>(args.User))
        {
            if (!TryComp<CondemnedComponent>(target, out var condemned)
                || condemned.SoulOwner != args.User)
            {
                condemned = EnsureComp<CondemnedComponent>(target);
                condemned.SoulOwner = args.User;
                condemned.SoulOwnedNotDevil = false;
                condemned.CondemnOnDeath = true;

                _condemned.StartCondemnation(
                    target,
                    freezeEntity: true,
                    doFlavor: true,
                    behavior: CondemnedBehavior.Delete
                );
            }
        }

        CleanupGrip(args.User, devilComp, ent);
        args.Handled = true;
    }

    private void CleanupGrip(EntityUid user, DevilComponent devilComp, Entity<DevilGripComponent> ent)
    {
        _actions.SetCooldown(devilComp.DevilGrip, ent.Comp.CooldownAfterUse);
        devilComp.DevilGrip = null;
        InvokeGrasp(user, ent);
        QueueDel(ent);
    }

    public void InvokeGrasp(EntityUid user, Entity<DevilGripComponent> ent)
    {
        _audio.PlayPvs(ent.Comp.Sound, user);
        _chat.TrySendInGameICMessage(user, Loc.GetString(ent.Comp.Invocation), InGameICChatType.Speak, false);
    }
}
