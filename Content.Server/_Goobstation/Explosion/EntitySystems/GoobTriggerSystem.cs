using Content.Server._Goobstation.Explosion.Components;
using Content.Server.Explosion.EntitySystems;

namespace Content.Server._Goobstation.Explosion.EntitySystems;

public sealed partial class GoobTriggerSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DeleteParentOnTriggerComponent, TriggerEvent>(HandleDeleteParentTrigger);
    }

    private void HandleDeleteParentTrigger(Entity<DeleteParentOnTriggerComponent> entity, ref TriggerEvent args)
    {
        if (!TryComp<TransformComponent>(entity, out var xform))
            return;

        EntityManager.QueueDeleteEntity(xform.ParentUid);
        args.Handled = true;
    }
}
