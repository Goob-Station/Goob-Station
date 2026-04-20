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

namespace Content.Goobstation.Server.Xenobiology;

public sealed partial class SlimeEatCorpseSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CorpseEaterComponent, SlimeEatCorpseEvent>(OnSlimeEatCorpseAttempt);
    }

    private void OnSlimeEatCorpseAttempt(Entity<CorpseEaterComponent> ent, ref SlimeEatCorpseEvent args)
    {
        if (TerminatingOrDeleted(args.Target)
        || TerminatingOrDeleted(args.Performer))
            return;

        if (!TryComp<BodyComponent>(args.Target, out var body)
            || !_body.TryGetRootPart(args.Target, out var rootPart))
            return;

        if (!_body.GetBodyContainers(args.Target, body, rootPart).Any(container => container.ContainedEntities.Any(slot => IsEatableOrganOrBodyPart(ent, slot))))
        {
            var notEatablePopup = Loc.GetString("slime-eat-corpse-fail-not-eatable", ("target", args.Target));
            _popup.PopupEntity(notEatablePopup, ent, ent);
            return;
        }

        if (!_mobState.IsDead(args.Target))
        {
            var notDeadPopup = Loc.GetString("slime-eat-corpse-fail-not-dead", ("target", args.Target));
            _popup.PopupEntity(notDeadPopup, ent, ent);
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
