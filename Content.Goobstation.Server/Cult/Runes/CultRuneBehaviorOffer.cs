using Content.Goobstation.Shared.Cult;
using Content.Server.Antag.Components;
using Content.Shared.Chat;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs;
using Robust.Shared.Player;
using Content.Goobstation.Server.Cult.GameTicking;
using Content.Server.Administration.Systems;
using Content.Server.Antag;
using Robust.Shared.Random;
using Content.Server.Flash;
using System.Linq;
using Content.Shared.Mindshield.Components;
using Content.Goobstation.Common.Religion;

namespace Content.Goobstation.Server.Cult.Runes;

public sealed partial class CultRuneBehaviorOffer : CultRuneBehavior
{
    private BloodCultRuleSystem _cultRuleSystem = default!;
    private AntagSelectionSystem _antag = default!;
    private IRobustRandom _rand = default!;
    private FlashSystem _flash = default!;
    private RejuvenateSystem _rejuv = default!;
    private SharedChatSystem _chat = default!;

    private Entity<BloodCultRuleComponent>? _rule;

    public override void Initialize(IEntityManager ent)
    {
        base.Initialize(ent);

        _cultRuleSystem = ent.System<BloodCultRuleSystem>();
        _antag = ent.System<AntagSelectionSystem>();
        _rand = IoCManager.Resolve<IRobustRandom>();
        _flash = ent.System<FlashSystem>();
        _rejuv = ent.System<RejuvenateSystem>();
        _chat = ent.System<SharedChatSystem>();

        _cultRuleSystem.TryGetRule(out _rule);
    }

    public override bool IsValid(IEntityManager ent, List<EntityUid> invokers, List<EntityUid> targets, out string invalidReason)
    {
        if (!base.IsValid(ent, invokers, targets, out invalidReason))
            return false;

        if (targets.Count == 0)
        {
            invalidReason = Loc.GetString("rune-invoke-fail-notarget");
            return false;
        }

        var target = targets.First();
        if (ent.TryGetComponent<MobStateComponent>(target, out var mob) && mob.CurrentState == MobState.Dead // dead
        || ent.HasComponent<MindShieldComponent>(target) // shielded
        || ent.HasComponent<BibleUserComponent>(target)) // chaplain
            return false;

        return true;
    }

    public override void Invoke(IEntityManager ent, List<EntityUid> invokers, List<EntityUid> targets, EntityUid? owner = null)
    {
        var target = targets.First();
        if (_rule.HasValue
        && ent.TryGetComponent<ActorComponent>(target, out var actor)
        && ent.TryGetComponent<AntagSelectionComponent>(_rule, out var asc))
            // you make the AntagSelectionComponent too sweet for me to not be using this
            _antag.MakeAntag((_rule.Value, asc), actor.PlayerSession, asc.Definitions.First());
#if DEBUG
        // calling it directly because brainless urist can't be ontag
        else _cultRuleSystem.MakeCultist(target, _rule!.Value);
#endif

#if !DEBUG
        // 1 in 1000 chance of the offeree saying the funny
        // guaranteed on debug!:tm:
        if (_rand.Prob(.001f))
#endif
        _chat.TrySendInGameICMessage(target, Loc.GetString("rune-invoke-offering-funny"), InGameICChatType.Speak, false);

        _flash.Flash(target, null, null, TimeSpan.FromSeconds(3), 0, displayPopup: false, stunDuration: TimeSpan.FromSeconds(1));
        _rejuv.PerformRejuvenate(target);
    }
}
