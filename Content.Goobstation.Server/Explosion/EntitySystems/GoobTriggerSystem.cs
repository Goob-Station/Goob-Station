using Content.Goobstation.Server.Explosion.Components;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;

namespace Content.Goobstation.Server.Explosion.EntitySystems;

public sealed partial class GoobTriggerSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DeleteParentOnTriggerComponent, TriggerEvent>(HandleDeleteParentTrigger);
        SubscribeLocalEvent<DropOnTriggerComponent, TriggerEvent>(HandleDropOnTrigger);
    }

    private void HandleDeleteParentTrigger(Entity<DeleteParentOnTriggerComponent> entity, ref TriggerEvent args)
    {
        EntityManager.QueueDeleteEntity(Transform(entity).ParentUid); // cleanedup - goob mudles
        args.Handled = true;
    }

    private void HandleDropOnTrigger(Entity<DropOnTriggerComponent> entity, ref TriggerEvent args)
    {
        if (!TryComp(entity, out HandsComponent? hands))
            return;

        foreach (var hand in _hands.EnumerateHands(entity, hands))
        {
            if (hand.HeldEntity == null)
                continue;

            _hands.TryDrop(entity, hand, handsComp: hands);
        }
        args.Handled = true;
    }
}
