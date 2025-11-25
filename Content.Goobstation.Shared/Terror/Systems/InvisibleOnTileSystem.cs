using Content.Goobstation.Shared.Terror.Components;
using Content.Shared.Spider;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Content.Shared.StepTrigger.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Terror.Systems;

public sealed class InvisibleOnTileSystem : EntitySystem
{
    [Dependency] private readonly SharedStealthSystem _stealth = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    // Tracks remaining invisibility time per entity
    private readonly Dictionary<EntityUid, float> _timers = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<InvisibleOnTileComponent, StepTriggeredOnEvent>(OnStepOn);
    }

    private void OnStepOn(EntityUid uid, InvisibleOnTileComponent comp, ref StepTriggeredOnEvent args)
    {
        // uid = web entity
        // args.Tripper = entity stepping on the web

        var tripper = args.Tripper;

        if (!HasComp<GrayTerrorComponent>(tripper))
            return;

        if (Deleted(tripper))
            return;

        // Refresh remaining time
        _timers[tripper] = (float) _timing.CurTime.TotalSeconds + comp.ExpireTime;

        // Apply or refresh stealth
        var stealth = EnsureComp<StealthComponent>(tripper);
        _stealth.SetVisibility(tripper, comp.Invisibility, stealth);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_timers.Count == 0)
            return;

        var now = (float) _timing.CurTime.TotalSeconds;

        // Collect entities that should expire
        List<EntityUid>? toRemove = null;

        foreach (var (ent, expireAt) in _timers)
        {
            if (now >= expireAt)
            {
                toRemove ??= new();
                toRemove.Add(ent);
            }
        }

        if (toRemove == null)
            return;

        // Expire invisibility
        foreach (var ent in toRemove)
        {
            _timers.Remove(ent);

            if (!Deleted(ent))
                RemCompDeferred<StealthComponent>(ent);
        }
    }
}
