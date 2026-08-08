using Content.Goobstation.Shared.Slasher.Components;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Slasher.Systems;

/// <summary>
/// Handles the lifetime of the overlay.
/// </summary>
public sealed class SharedSlasherRegenerateOverlaySystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherRegenerateOverlayComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<SlasherRegenerateOverlayComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.EndTime = _timing.CurTime + ent.Comp.Duration;
        Dirty(ent);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<SlasherRegenerateOverlayComponent>();
        while (query.MoveNext(out var uid, out var overlay))
        {
            if (_timing.CurTime >= overlay.EndTime)
                RemCompDeferred<SlasherRegenerateOverlayComponent>(uid);
        }
    }
}
