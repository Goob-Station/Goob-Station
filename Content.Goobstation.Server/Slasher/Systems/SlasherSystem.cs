using Content.Goobstation.Common.Body;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Server.Body.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Rejuvenate;
using Content.Shared.Standing;
using Robust.Shared.Containers;

namespace Content.Goobstation.Server.Slasher.Systems;

/// <summary>
/// Moves the slasher's brain from the head into the chest
/// </summary>
public sealed class SlasherSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SlasherComponent, RejuvenateEvent>(OnRejuvenate);
        SubscribeLocalEvent<SlasherComponent, BeforeBrainRemovedEvent>(OnBeforeBrainRemoved);
    }

    private void OnMapInit(Entity<SlasherComponent> ent, ref MapInitEvent args) =>
        MoveBrainToChest(ent);

    private void OnRejuvenate(Entity<SlasherComponent> ent, ref RejuvenateEvent args) =>
        MoveBrainToChest(ent);

    private void OnBeforeBrainRemoved(Entity<SlasherComponent> ent, ref BeforeBrainRemovedEvent args) =>
        args.Blocked = true;

    private void MoveBrainToChest(Entity<SlasherComponent> ent)
    {
        if (!TryComp<BodyComponent>(ent, out var bodyComp)
            || !_body.TryGetBodyOrganEntityComps<BrainComponent>((ent.Owner, bodyComp), out var brains))
            return;

        var (brain, _, organ) = brains[0];
        if (!IsInHead(brain))
            return;

        EntityUid? chestPart = null;
        foreach (var (partId, _) in _body.GetBodyChildrenOfType(ent.Owner, BodyPartType.Chest, bodyComp))
        {
            chestPart = partId;
            break;
        }

        if (chestPart == null)
            return;

        var slotId = organ.SlotId;
        _body.RemoveOrgan(brain, organ);
        _body.TryCreateOrganSlot(chestPart, slotId, out _);
        _body.InsertOrgan(chestPart.Value, brain, slotId);
        _standing.Stand(ent.Owner, force: true);
    }

    private bool IsInHead(EntityUid organ)
    {
        return _container.TryGetContainingContainer((organ, null, null), out var container)
               && TryComp<BodyPartComponent>(container.Owner, out var part)
               && part.PartType == BodyPartType.Head;
    }
}
