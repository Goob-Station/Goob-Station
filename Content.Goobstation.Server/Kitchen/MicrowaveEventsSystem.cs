using Content.Server.Kitchen.Components;
using Robust.Shared.Containers;

namespace Content.Goobstation.Server.Kitchen;

/// <summary>
/// Prevents automation taking items out of an active microwave.
/// Only exists because microwave supercode only prevents it in interaction, not attempt events.
/// </summary>
public sealed class MicrowaveEventsSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActiveMicrowaveComponent, ContainerIsRemovingAttemptEvent>(OnRemoveAttempt);
    }

    private void OnRemoveAttempt(Entity<ActiveMicrowaveComponent> ent, ref ContainerIsRemovingAttemptEvent args)
    {
        args.Cancel();
    }
}
