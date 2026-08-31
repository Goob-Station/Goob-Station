using Content.Goobstation.Common.Body;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Rejuvenate;
using Content.Shared.Standing;
using Robust.Shared.Containers;

namespace Content.Goobstation.Server.Slasher.Systems;

/// <summary>
/// Moves the slasher's brain from the head into the chest.
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

        var brain = brains[0];
        var organ = brain.Comp2;

        if (!_container.TryGetContainingContainer((brain.Owner, null, null), out var current)
            || !TryComp<BodyPartComponent>(current.Owner, out var currentPart)
            || currentPart.PartType != BodyPartType.Head)
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
        _body.RemoveOrgan(brain.Owner, organ);
        _body.TryCreateOrganSlot(chestPart, slotId, out _, null);
        _body.InsertOrgan(chestPart.Value, brain.Owner, slotId);
        _standing.Stand(ent.Owner, force: true);
    }
}
