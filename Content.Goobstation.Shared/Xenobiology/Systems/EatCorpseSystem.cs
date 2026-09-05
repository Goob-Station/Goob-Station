using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using System.Linq;
using Content.Shared.Popups;
using Content.Shared.Mobs.Systems;
using Content.Shared.Whitelist;
using Content.Shared.DoAfter;
using Content.Shared.Jittering;
using Content.Shared.Gibbing.Events;
using Content.Shared.StatusEffectNew;
using Content.Shared.Mobs.Components;

namespace Content.Goobstation.Shared.Xenobiology.Systems;

public sealed partial class EatCorpseSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedJitteringSystem _jitter = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;

    private EntityQuery<OrganComponent> _organQuery;
    private EntityQuery<BodyComponent> _bodyQuery;
    private EntityQuery<BodyPartComponent> _bodyPartQuery;

    public override void Initialize()
    {
        SubscribeLocalEvent<CorpseEaterComponent, EatCorpseEvent>(OnEatCorpseAttempt);
        SubscribeLocalEvent<CorpseEaterComponent, EatCorpseDoAfterEvent>(OnEatCorpseDoAfterEvent);

        _organQuery = GetEntityQuery<OrganComponent>();
        _bodyQuery = GetEntityQuery<BodyComponent>();
        _bodyPartQuery = GetEntityQuery<BodyPartComponent>();
    }

    private void OnEatCorpseAttempt(Entity<CorpseEaterComponent> eater, ref EatCorpseEvent args)
    {
        if (TerminatingOrDeleted(args.Target)
            || TerminatingOrDeleted(args.Performer))
            return;

        TryEatCorpse(eater.Owner, args.Target, eater.Comp);
    }

    public bool CanEatCorpse(EntityUid eaterUid,
        EntityUid targetUid,
        CorpseEaterComponent? eater = null,
        BodyComponent? targetBody = null,
        MobStateComponent? targetState = null)
    {
        if (!Resolve(eaterUid, ref eater, false)
            || !Resolve(targetUid, ref targetState, ref targetBody, false))
            return false;

        if (!_mobState.IsDead(targetUid))
            return false;

        if (!_body.GetBodyOrgans(targetUid, targetBody).Any(organ => IsValidOrganOrBodyPart(eater, organ.Id))
            && !_body.GetBodyChildren(targetUid, targetBody).Any(part => IsValidOrganOrBodyPart(eater, part.Id)))
            return false;

        return true;
    }

    public bool TryEatCorpse(EntityUid eaterUid,
        EntityUid targetUid,
        CorpseEaterComponent? eater = null,
        BodyComponent? targetBody = null,
        MobStateComponent? targetState = null)
    {
        if (!Resolve(eaterUid, ref eater, false)
            || !Resolve(targetUid, ref targetState, ref targetBody, false))
            return false;

        if (!_body.TryGetRootPart(targetUid, out var _, targetBody))
            return false;

        if (!CanEatCorpse(eaterUid, targetUid, eater, targetBody))
        {
            var fail = Loc.GetString("slime-eat-corpse-fail", ("target", targetUid));
            _popup.PopupEntity(fail, eaterUid, PopupType.Small);
            return false;
        }

        var doAfterArgs = new DoAfterArgs(EntityManager, eaterUid, eater.EatCorpseDoAfterDuration, new EatCorpseDoAfterEvent(), eaterUid, targetUid)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            DuplicateCondition = DuplicateConditions.SameTool, // multiple slimes can eat one target, but one slime can't eat multiple targets
        };

        EnsureComp<BeingEatenComponent>(targetUid); // Dont let slime interupt each other

        if (!_doAfter.TryStartDoAfter(doAfterArgs, out eater.LastDoAfterId))
        {
            RemComp<BeingEatenComponent>(targetUid);
            return false;
        }

        _jitter.DoJitter(targetUid, eater.EatCorpseDoAfterDuration, true);
        var attemptPopup = Loc.GetString("slime-eat-corpse-success", ("eater", eaterUid), ("target", targetUid));
        _popup.PopupEntity(attemptPopup, eaterUid, PopupType.MediumCaution);

        return true;
    }

    private void OnEatCorpseDoAfterEvent(Entity<CorpseEaterComponent> eater, ref EatCorpseDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target is not { } target)
        {
            if (args.Target is { } cancelledTarget)
            {
                _statusEffects.TryRemoveStatusEffect(cancelledTarget, "Jitter");
                RemComp<BeingEatenComponent>(cancelledTarget);
            }

            args.Handled = true;
            return;
        }

        if (!_bodyQuery.TryComp(target, out var body)
            || !_body.TryGetRootPart(target, out var rootPart, body))
        {
            RemComp<BeingEatenComponent>(target);
            return;
        }

        // TODO: randomize body parts or give a choice of which to tear off
        // we want to remove parts from the furthest from root to the nearest and remove organs of part before part itself
        var partsAndOrgans = _body.GetBodyChildren(target, body, rootPart).SelectMany(part => _body.GetPartOrgans(part.Id, part.Component).Select(organ => organ.Id).Prepend(part.Id));
        var toRemove = partsAndOrgans.Reverse().FirstOrDefault(x => IsValidOrganOrBodyPart(eater, x), EntityUid.Invalid);

        if (toRemove == EntityUid.Invalid)
        {
            RemComp<BeingEatenComponent>(target);
            return;
        }

        if (toRemove == rootPart.Value.Owner)
        {
            _body.GibBody(target, gib: GibType.Drop);
            RemComp<BeingEatenComponent>(target);
            return;
        }

        _body.RemoveOrgan(toRemove);
        _body.TryDetachPart(toRemove);
        RemComp<BeingEatenComponent>(target);
    }

    private bool IsValidOrganOrBodyPart(CorpseEaterComponent eater, EntityUid target)
    {
        if (_organQuery.HasComp(target))
            return _whitelist.CheckBoth(target, eater.OrganBlacklist, eater.OrganWhitelist);

        if (_bodyPartQuery.TryComp(target, out var part))
            return part.PartComposition == eater.BodyPartComposition || eater.BodyPartComposition is null
                && _whitelist.CheckBoth(target, eater.BodyPartBlacklist, eater.BodyPartWhitelist);

        return false;
    }
}
