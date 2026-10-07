// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Devil;
using Content.Goobstation.Shared.Religion;
using Content.Server.Chat.Systems;
using Content.Server.Speech.EntitySystems;
using Content.Shared.Actions;
using Content.Shared.Chat;
using Content.Shared.Damage.Systems;
using Content.Shared.Interaction;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Server.Devil.Grip;

public sealed partial class DevilGripSystem : EntitySystem
{
    [Dependency] private SharedStaminaSystem _stamina = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private ChatSystem _chat = default!;
    [Dependency] private RatvarianLanguageSystem _language = default!;
    [Dependency] private EntityWhitelistSystem _whitelist = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private DivineInterventionSystem _divineIntervention = default!;

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
            || _whitelist.IsWhitelistPass(ent.Comp.Blacklist, target)
            || !TryComp<DevilComponent>(args.User, out var devilComp))
            return;

        if (_divineIntervention.ShouldDeny(target))
        {
            _actions.SetCooldown(devilComp.DevilGrip, ent.Comp.CooldownAfterUse);
            devilComp.DevilGrip = null;
            InvokeGrasp(args.User, ent);
            QueueDel(ent);
            args.Handled = true;
            return;
        }

        if (TryComp(target, out StatusEffectsComponent? status))
        {
            _stun.KnockdownOrStun(target, ent.Comp.KnockdownTime, true);
            _stamina.TakeStaminaDamage(target, ent.Comp.StaminaDamage);
            _language.DoRatvarian(target, ent.Comp.SpeechTime, true, status);
        }

        _actions.SetCooldown(devilComp.DevilGrip, ent.Comp.CooldownAfterUse);
        devilComp.DevilGrip = null;
        InvokeGrasp(args.User, ent);
        QueueDel(ent);
        args.Handled = true;
    }

    public void InvokeGrasp(EntityUid user, Entity<DevilGripComponent> ent)
    {
        _audio.PlayPvs(ent.Comp.Sound, user);
        _chat.TrySendInGameICMessage(user, Loc.GetString(ent.Comp.Invocation), InGameICChatType.Speak, false);
    }
}
