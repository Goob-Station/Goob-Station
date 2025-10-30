using Content.Goobstation.Common.Grab;
using Content.Goobstation.Shared.Grab;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Movement.Pulling.Events;

namespace Content.Goobstation.Shared.Held;

public sealed class GoobHandsSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HandsComponent, StopGrabbingItemPullEvent>(RefRelayEvent);
        SubscribeLocalEvent<HandsComponent, FindGrabbingItemEvent>(RefRelayEvent);
        SubscribeLocalEvent<HandsComponent, GrabModifierEvent>(RefRelayEvent);
    }

    private void RelayEvent<T>(Entity<HandsComponent> entity, ref T args) where T : EntityEventArgs
    {
        CoreRelayEvent(entity, ref args);
    }

    private void RefRelayEvent<T>(Entity<HandsComponent> entity, ref T args)
    {
        var ev = CoreRelayEvent(entity, ref args);
        args = ev.Args;
    }

    private HeldRelayedEvent<T> CoreRelayEvent<T>(Entity<HandsComponent> entity, ref T args)
    {
        var ev = new HeldRelayedEvent<T>(args);

        foreach (var held in _hands.EnumerateHeld(entity.AsNullable()))
        {
            RaiseLocalEvent(held, ref ev);
        }

        return ev;
    }
}
