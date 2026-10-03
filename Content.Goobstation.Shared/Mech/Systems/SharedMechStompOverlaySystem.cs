using Content.Goobstation.Shared.Mech.Components;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Mech.Systems;

public sealed class SharedMechStompOverlaySystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MechStompOverlayComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<MechStompOverlayComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.EndTime = _timing.CurTime + ent.Comp.Duration;
        Dirty(ent);
    }

    public override void Update(float frameTime)
    {
        if (_net.IsClient)
            return;

        var query = EntityQueryEnumerator<MechStompOverlayComponent>();
        while (query.MoveNext(out var uid, out var overlay))
        {
            if (_timing.CurTime >= overlay.EndTime)
                RemCompDeferred<MechStompOverlayComponent>(uid);
        }
    }
}
