using System.Configuration;
using Content.Goobstation.Server.Explosion.Components;
using Content.Server._Shitmed.DelayedDeath;
using Content.Server.Explosion.EntitySystems;

namespace Content.Goobstation.Server.Explosion.EntitySystems;

public sealed partial class GoobTriggerSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DeleteParentOnTriggerComponent, TriggerEvent>(HandleDeleteParentTrigger);
    }

    private void HandleDeleteParentTrigger(Entity<DeleteParentOnTriggerComponent> entity, ref TriggerEvent args)
    {
        EntityManager.QueueDeleteEntity(Transform(entity).ParentUid);
        args.Handled = true;
    }
}


