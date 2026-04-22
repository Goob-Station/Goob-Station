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
using Robust.Shared.Containers;
using Content.Shared.Gibbing.Events;
using Content.Shared.StatusEffect;// TODO: change to StatusEffectNew when jittering would be migrated

namespace Content.Goobstation.Server.Xenobiology;

public sealed partial class SlimeEatCorpseSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedJitteringSystem _jitter = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CorpseEaterComponent, SlimeEatCorpseEvent>(OnSlimeEatCorpseAttempt);
        SubscribeLocalEvent<CorpseEaterComponent, DoAfterAttemptEvent<EatCorpseDoAfterEvent>>(OnDoAfterAttempt);
        SubscribeLocalEvent<CorpseEaterComponent, EatCorpseDoAfterEvent>(OnEatCorpseDoAfterEvent);
    }

    private void OnSlimeEatCorpseAttempt(Entity<CorpseEaterComponent> eater, ref SlimeEatCorpseEvent args)
    {
        var target = args.Target;
        if (TerminatingOrDeleted(target)
        || TerminatingOrDeleted(args.Performer))
            return;

        if (!TryComp<BodyComponent>(target, out var body)
            || !_body.TryGetRootPart(target, out var rootPart, body))
            return;

        if (!_body.GetBodyOrgans(target, body).Any(organ => IsEatableOrganOrBodyPart(eater, organ.Id))
            && !_body.GetBodyChildren(target, body, rootPart).Any(part => IsEatableOrganOrBodyPart(eater, part.Id)))
        {
            var notEatablePopup = Loc.GetString("slime-eat-corpse-fail-not-eatable", ("target", target));
            _popup.PopupEntity(notEatablePopup, eater, eater);
            return;
        }

        if (!_mobState.IsDead(target))
        {
            var notDeadPopup = Loc.GetString("slime-eat-corpse-fail-not-dead", ("target", target));
            _popup.PopupEntity(notDeadPopup, eater, eater);
            return;
        }

        var doAfterArgs = new DoAfterArgs(EntityManager, eater, eater.Comp.EatCorpseDoAfterDuration, new EatCorpseDoAfterEvent(), eater, target)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
        };

        if (!_doAfter.TryStartDoAfter(doAfterArgs))
            return;

        _jitter.DoJitter(target, eater.Comp.EatCorpseDoAfterDuration, true);
        var attemptPopup = Loc.GetString("slime-latch-attempt", ("slime", eater), ("ent", target));
        _popup.PopupEntity(attemptPopup, eater, PopupType.MediumCaution);
    }

    private void OnDoAfterAttempt(Entity<CorpseEaterComponent> _, ref DoAfterAttemptEvent<EatCorpseDoAfterEvent> args)
    {
        if (!args.Cancelled)
            return;

        if (args.DoAfter.Args.Target is not null)
            _jitter.DoJitter(args.DoAfter.Args.Target ?? default!, TimeSpan.FromSeconds(1), true);
    }

    private void OnEatCorpseDoAfterEvent(Entity<CorpseEaterComponent> eater, ref EatCorpseDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target is not { } target)
        {
            if (args.Target is { } cancelledTarget)
                _statusEffects.TryRemoveStatusEffect(cancelledTarget, "Jitter");
            return;
        }

        if (!TryComp<BodyComponent>(target, out var body)
            || !_body.TryGetRootPart(target, out var rootPart, body))
            return;

        // we want to remove parts from the furthest from root to the nearest and remove organs of part before part itself
        var partsAndOrgans = _body.GetBodyChildren(target, body, rootPart).SelectMany(part => _body.GetPartOrgans(part.Id, part.Component).Select(organ => organ.Id).Prepend(part.Id));
        var toRemove = partsAndOrgans.Reverse().FirstOrDefault(x => IsEatableOrganOrBodyPart(eater, x), EntityUid.Invalid);

        if (toRemove == EntityUid.Invalid)
            return;

        if (toRemove == rootPart.Value.Owner)
        {
            _body.GibBody(target, gib: GibType.Drop);
            return;
        }

        _body.RemoveOrgan(toRemove);
        _body.TryDetachPart(toRemove);
    }

    private bool IsEatableOrganOrBodyPart(Entity<CorpseEaterComponent> eater, EntityUid food)
    {
        if (!HasComp<EdibleComponent>(food))
            return false;

        if (HasComp<OrganComponent>(food))
            return _whitelist.CheckBoth(food, eater.Comp.OrganBlacklist, eater.Comp.OrganWhitelist);

        if (TryComp<BodyPartComponent>(food, out var part))
            return part.PartComposition == BodyPartComposition.Organic
                && _whitelist.CheckBoth(food, eater.Comp.BodyPartBlacklist, eater.Comp.BodyPartWhitelist);

        return false;
    }
}
