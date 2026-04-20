using Content.Goobstation.Shared.Xenobiology;
using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Shared.Body.Part;
using Content.Shared._Shitmed.Body.Part;
using Content.Shared.Body.Organ;
using Content.Server.Nutrition.Components;
using Content.Shared.Tag;
using Content.Shared.Nutrition.Components;
using Robust.Shared.Prototypes;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using System.Linq;
using Content.Shared.Popups;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Whitelist;
using Content.Shared.DoAfter;
using Content.Shared.Jittering;
using Content.Shared.StatusEffect; // TODO: change to StatusEffectNew when jittering would be migrated

namespace Content.Goobstation.Server.Xenobiology;

public sealed partial class SlimeEatCorpseSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedJitteringSystem _jitter = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CorpseEaterComponent, SlimeEatCorpseEvent>(OnSlimeEatCorpseAttempt);
        SubscribeLocalEvent<CorpseEaterComponent, DoAfterAttemptEvent<EatCorpseDoAfterEvent>>(OnDoAfterAttempt);
        SubscribeLocalEvent<CorpseEaterComponent, EatCorpseDoAfterEvent>(OnEatCorpseDoAfterEvent);
    }

    private void OnSlimeEatCorpseAttempt(Entity<CorpseEaterComponent> ent, ref SlimeEatCorpseEvent args)
    {
        var target = args.Target;
        if (TerminatingOrDeleted(target)
        || TerminatingOrDeleted(args.Performer))
            return;

        if (!TryComp<BodyComponent>(target, out var body)
            || !_body.TryGetRootPart(target, out var rootPart, body))
            return;

        if (!_body.GetBodyContainers(target, body, rootPart).Any(container => container.ContainedEntities.Any(slot => IsEatableOrganOrBodyPart(ent, slot))))
        {
            var notEatablePopup = Loc.GetString("slime-eat-corpse-fail-not-eatable", ("target", target));
            _popup.PopupEntity(notEatablePopup, ent, ent);
            return;
        }

        if (!_mobState.IsDead(target))
        {
            var notDeadPopup = Loc.GetString("slime-eat-corpse-fail-not-dead", ("target", target));
            _popup.PopupEntity(notDeadPopup, ent, ent);
            return;
        }

        var doAfterArgs = new DoAfterArgs(EntityManager, ent, ent.Comp.EatCorpseDoAfterDuration, new EatCorpseDoAfterEvent(), ent, target)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
        };

        if (!_doAfter.TryStartDoAfter(doAfterArgs))
            return;

        _jitter.DoJitter(target, ent.Comp.EatCorpseDoAfterDuration, true);
        var attemptPopup = Loc.GetString("slime-latch-attempt", ("slime", ent), ("ent", target));
        _popup.PopupEntity(attemptPopup, ent, PopupType.MediumCaution);
    }

    private void OnDoAfterAttempt(Entity<CorpseEaterComponent> _, ref DoAfterAttemptEvent<EatCorpseDoAfterEvent> args)
    {
        if (!args.Cancelled)
            return;

        if (args.DoAfter.Args.Target is not null)
            _jitter.DoJitter(args.DoAfter.Args.Target ?? default!, TimeSpan.FromSeconds(1), true);
    }

    private void OnEatCorpseDoAfterEvent(Entity<CorpseEaterComponent> xeno, ref EatCorpseDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target is not { } target)
        {
            if (args.Target is { } cancelledTarget)
                _statusEffects.TryRemoveStatusEffect(cancelledTarget, "Jitter");
            return;
        }
    }

    private bool IsEatableOrganOrBodyPart(Entity<CorpseEaterComponent> eater, EntityUid food)
    {
        if (!HasComp<EdibleComponent>(food))
            return false;

        if (HasComp<BadFoodComponent>(food))
            return false;

        if (HasComp<OrganComponent>(food))
            return _whitelist.CheckBoth(food, eater.Comp.OrganWhitelist, eater.Comp.OrganBlacklist);

        if (TryComp<BodyPartComponent>(food, out var part))
            return part.PartComposition == BodyPartComposition.Organic
                && _whitelist.CheckBoth(food, eater.Comp.BodyPartWhitelist, eater.Comp.BodyPartBlacklist);

        return false;
    }
}
