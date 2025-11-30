using Content.Goobstation.Server.TargetEvents.Components;
using Content.Server.Explosion.EntitySystems;

namespace Content.Goobstation.Server.TargetEvents.EntitySystems;

public sealed class ExecuteTargetEventsOnTriggerSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExecuteTargetEventsOnTriggerComponent, TriggerEvent>(OnTrigger);
    }

    private void OnTrigger(Entity<ExecuteTargetEventsOnTriggerComponent> entity, ref TriggerEvent ev)
    {
        foreach (var targetEvent in entity.Comp.Events)
        {
            targetEvent.Target = entity;
            RaiseLocalEvent(entity, (object) targetEvent, true);
        }
    }
}
