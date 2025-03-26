using Content.Server._Goobstation.Explosion.Components;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;

namespace Content.Server._Goobstation.Explosion.EntitySystems;

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
        if (!TryComp<TransformComponent>(entity, out var xform))
            return;

        EntityManager.QueueDeleteEntity(xform.ParentUid);
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
