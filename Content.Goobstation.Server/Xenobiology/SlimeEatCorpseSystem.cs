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

namespace Content.Goobstation.Server.Xenobiology;

public sealed partial class SlimeEatCorpseSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    private readonly ProtoId<TagPrototype> _slimeTag = "Slime";

    public override void Initialize()
    {
        SubscribeLocalEvent<CorpseEaterComponent, SlimeEatCorpseEvent>(OnSlimeEatCorpseAttempt);
    }

    private void OnSlimeEatCorpseAttempt(Entity<CorpseEaterComponent> ent, ref SlimeEatCorpseEvent args)
    {
        if (TerminatingOrDeleted(args.Target)
        || TerminatingOrDeleted(args.Performer))
            return;

        TryComp<BodyPartComponent>(args.Target, out var part);

        var targetPart = args.Target;

        if (TryComp<BodyComponent>(args.Target, out var body)
            && body.RootContainer.ContainedEntity is not null
            && TryComp<BodyPartComponent>(body.RootContainer.ContainedEntity, out var bodyPart))
        {
            part = bodyPart;
            targetPart = (EntityUid) body.RootContainer.ContainedEntity;
        }

        if (part is null)
            return;

        if (!_body.GetPartContainers(targetPart).Any(x => x.ContainedEntities.Any(y => IsEatableOrganOrBodyPart(ent, y))))
        {
            var notEatablePopup = Loc.GetString("slime-eat-corpse-fail-not-eatable", ("target", args.Target));
            _popup.PopupEntity(notEatablePopup, ent, ent);
            return;
        }

        if (TryComp<MobStateComponent>(args.Target, out var mobState)
            && !_mobState.IsDead(args.Target, mobState))
        {
            var notDeadPopup = Loc.GetString("slime-eat-corpse-fail-not-dead", ("target", args.Target));
            _popup.PopupEntity(notDeadPopup, ent, ent);
            return;
        }
    }

    private bool IsEatableOrganOrBodyPart(Entity<CorpseEaterComponent> eater, EntityUid food)
    {
        CorpseEaterComponent
        if (HasComp<BadFoodComponent>(food))
            return false;

        var notDeadPopup = Loc.GetString(food.ToString());
        _popup.PopupEntity(notDeadPopup, food);

        if (HasComp<OrganComponent>(food))
            return HasComp<EdibleComponent>(food)
                && !_tag.HasTag(food, _slimeTag);

        if (TryComp<BodyPartComponent>(food, out var part))
            return part.PartComposition == BodyPartComposition.Organic;

        return false;
    }
}
