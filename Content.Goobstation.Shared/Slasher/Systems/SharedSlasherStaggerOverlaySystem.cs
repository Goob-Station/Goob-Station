using Content.Goobstation.Shared.Slasher.Components;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Slasher.Systems;

/// <summary>
/// Handles the lifetime of the stagger area component.
/// </summary>
public sealed class SharedSlasherStaggerOverlaySystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherStaggerOverlayComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<SlasherStaggerOverlayComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.EndTime = _timing.CurTime + ent.Comp.Duration;
        Dirty(ent);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<SlasherStaggerOverlayComponent>();
        while (query.MoveNext(out var uid, out var overlay))
        {
            if (_timing.CurTime >= overlay.EndTime)
                RemCompDeferred<SlasherStaggerOverlayComponent>(uid);
        }
    }
}
