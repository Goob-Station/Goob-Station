using Content.Server._CorvaxGoob.Events.Components;
using Content.Server.Explosion.EntitySystems;

namespace Content.Server._CorvaxGoob.Events.EntitySystems;

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
