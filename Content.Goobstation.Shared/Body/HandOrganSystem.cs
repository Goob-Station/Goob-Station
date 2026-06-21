using Content.Goobstation.Shared.Body;

private void OnGotInserted(Entity<HandOrganComponent> ent, ref OrganGotInsertedEvent args)
{
    // <Trauma>
    _hands.AddHand(args.Target.Owner, ent.Comp.HandID, ent.Comp.Data);

    if (ent.Comp.StartingItem is not { } proto)
        return;
    var item = PredictedSpawnNextToOrDrop(proto, args.Target);
    _hands.TryPickup(args.Target.Owner, item, ent.Comp.HandID, animate: false);
    // </Trauma>
}

private void OnGotRemoved(Entity<HandOrganComponent> ent, ref OrganGotRemovedEvent args)
