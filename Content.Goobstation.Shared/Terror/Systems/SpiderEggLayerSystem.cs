using Content.Goobstation.Shared.Terror.Components;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Terror.Systems;

public sealed class SpiderEggLayerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpiderEggLayerComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, SpiderEggLayerComponent comp, MapInitEvent args)
    {
        comp.NextGenerationTime = _timing.CurTime + comp.GenerationInterval;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SpiderEggLayerComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.StoredEggs >= comp.MaxStoredEggs)
                continue;

            if (_timing.CurTime < comp.NextGenerationTime)
                continue;

            comp.StoredEggs++;

            // TO DO: Add pop-up

            comp.NextGenerationTime = _timing.CurTime + comp.GenerationInterval;
            Dirty(uid, comp);
        }
    }

    // Adds eggs directly, bypassing the timer. Returns the new stored count. This is for the green terror, which
    // does not generate eggs over time.
    public int AddEgg(EntityUid uid, int amount = 1, SpiderEggLayerComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return 0;

        comp.StoredEggs = Math.Min(comp.StoredEggs + amount, comp.MaxStoredEggs);

        // TO DO: Add pop-up

        Dirty(uid, comp);
        return comp.StoredEggs;
    }

    // Attempts to consume one stored egg. Returns false if none are available.
    public bool TryConsumeEgg(EntityUid uid, SpiderEggLayerComponent? comp = null)
    {
        if (!Resolve(uid, ref comp, false))
            return false;

        if (comp.StoredEggs <= 0)
        {
            // TO DO: Add pop-up

            return false;
        }

        comp.StoredEggs--;

        // TO DO: Add pop-up

        Dirty(uid, comp);
        return true;
    }
}
