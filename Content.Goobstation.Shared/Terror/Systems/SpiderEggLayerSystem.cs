using Content.Goobstation.Shared.Terror.Components;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Terror.Systems;

public sealed class SpiderEggLayerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

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

            _popup.PopupPredicted(Loc.GetString("terror-gain-egg"), uid, uid);

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

        _popup.PopupPredicted(Loc.GetString("terror-gain-egg"), uid, uid);

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
            _popup.PopupPredicted(Loc.GetString("terror-no-eggs"), uid, uid);

            return false;
        }

        comp.StoredEggs--;

        _popup.PopupPredicted(Loc.GetString("terror-lay-egg"), uid, uid);

        Dirty(uid, comp);
        return true;
    }
}
