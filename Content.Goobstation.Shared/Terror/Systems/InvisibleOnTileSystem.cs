using Content.Goobstation.Shared.Terror.Components;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Content.Shared.StepTrigger.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Terror.Systems;

/// <summary>
/// Makes an entity transparent on top of a tile.
/// </summary>
public sealed class InvisibleOnTileSystem : EntitySystem
{
    [Dependency] private readonly SharedStealthSystem _stealth = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

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

        if (!TryComp<TerrorSpiderComponent>(tripper, out var spider))
            return;

        var proto = _proto.Index(spider.SpiderType);

        if (!proto.IsInvisibleOnWeb)
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
