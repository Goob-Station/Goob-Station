using Content.Shared.Body;
using Content.Shared.Hands.EntitySystems;

namespace Content.Goobstation.Shared.Body;

public sealed class HandOrganSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HandOrganComponent, OrganGotInsertedEvent>(OnGotInserted);
        SubscribeLocalEvent<HandOrganComponent, OrganGotRemovedEvent>(OnGotRemoved);
    }

    private void OnGotInserted(Entity<HandOrganComponent> ent, ref OrganGotInsertedEvent args)
    {
        // <Trauma>
        _hands.AddHand(args.Target.Owner, ent.Comp.HandID, ent.Comp.Data);

        if (ent.Comp.StartingItem is not { } proto) return;
        var item = PredictedSpawnNextToOrDrop(proto, args.Target);
        _hands.TryPickup(args.Target.Owner, item, ent.Comp.HandID, animate: false);
        // </Trauma>
    }

    private void OnGotRemoved(Entity<HandOrganComponent> ent, ref OrganGotRemovedEvent args)
    {
        // prevent a recursive double-delete bug
        if (LifeStage(args.Target) >= EntityLifeStage.Terminating)
            return;

        _hands.RemoveHand(args.Target.Owner, ent.Comp.HandID); // Trauma - use .Owner
    }
}
