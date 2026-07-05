using Content.Goobstation.Shared.Xenobiology;
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
using Content.Shared.StatusEffect;
using Content.Shared.Mobs.Components;// TODO: change to StatusEffectNew when jittering would be migrated

namespace Content.Goobstation.Server.Xenobiology;

public sealed partial class EatCorpseSystem : EntitySystem
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
        SubscribeLocalEvent<CorpseEaterComponent, EatCorpseEvent>(OnEatCorpseAttempt);
        SubscribeLocalEvent<CorpseEaterComponent, EatCorpseDoAfterEvent>(OnEatCorpseDoAfterEvent);
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
        if (!Resolve(eaterUid, ref eater)
            || !Resolve(targetUid, ref targetState, ref targetBody))
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
        if (!Resolve(eaterUid, ref eater)
            || !Resolve(targetUid, ref targetState, ref targetBody))
            return false;

        if (!_body.TryGetRootPart(targetUid, out var rootPart, targetBody))
            return false;

        if (!_body.GetBodyOrgans(targetUid, targetBody).Any(organ => IsValidOrganOrBodyPart(eater, organ.Id))
            && !_body.GetBodyChildren(targetUid, targetBody, rootPart).Any(part => IsValidOrganOrBodyPart(eater, part.Id)))
        {
            var notEatablePopup = Loc.GetString("slime-eat-corpse-fail-not-eatable", ("target", targetUid));
            _popup.PopupEntity(notEatablePopup, eaterUid, eaterUid);
            return false;
        }

        if (!_mobState.IsDead(targetUid))
        {
            var notDeadPopup = Loc.GetString("slime-eat-corpse-fail-not-dead", ("target", targetUid));
            _popup.PopupEntity(notDeadPopup, eaterUid, eaterUid);
            return false;
        }

        if (!CanEatCorpse(eaterUid, targetUid, eater, targetBody)) // all conditions already above, but just in case
            return false;

        var doAfterArgs = new DoAfterArgs(EntityManager, eaterUid, eater.EatCorpseDoAfterDuration, new EatCorpseDoAfterEvent(), eaterUid, targetUid)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            DuplicateCondition = DuplicateConditions.SameTool, // multiple slimes can eat one target, but one slime can't eat multiple targets
        };

        if (!_doAfter.TryStartDoAfter(doAfterArgs, out eater.LastDoAfterId))
            return false;

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
                _statusEffects.TryRemoveStatusEffect(cancelledTarget, "Jitter");
            return;
        }

        if (!TryComp<BodyComponent>(target, out var body)
            || !_body.TryGetRootPart(target, out var rootPart, body))
            return;

        // TODO: randomize body parts or give a choice of which to tear off
        // we want to remove parts from the furthest from root to the nearest and remove organs of part before part itself
        var partsAndOrgans = _body.GetBodyChildren(target, body, rootPart).SelectMany(part => _body.GetPartOrgans(part.Id, part.Component).Select(organ => organ.Id).Prepend(part.Id));
        var toRemove = partsAndOrgans.Reverse().FirstOrDefault(x => IsValidOrganOrBodyPart(eater, x), EntityUid.Invalid);

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

    private bool IsValidOrganOrBodyPart(CorpseEaterComponent eater, EntityUid target)
    {
        if (HasComp<OrganComponent>(target))
            return _whitelist.CheckBoth(target, eater.OrganBlacklist, eater.OrganWhitelist);

        if (TryComp<BodyPartComponent>(target, out var part))
            return part.PartComposition == eater.BodyPartComposition || eater.BodyPartComposition is null
                && _whitelist.CheckBoth(target, eater.BodyPartBlacklist, eater.BodyPartWhitelist);

        return false;
    }
}
