using Content.Goobstation.Shared.Heretic;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;

namespace Content.Goobstation.Shared.Hands.EntitySystems;

public sealed partial class GoobSharedHandsSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HandsComponent, CheckMagicItemEvent>(RelayEvent);
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

        foreach (var held in _handsSystem.EnumerateHeld(entity, entity.Comp))
            RaiseLocalEvent(held, ref ev);

        return ev;
    }
}
