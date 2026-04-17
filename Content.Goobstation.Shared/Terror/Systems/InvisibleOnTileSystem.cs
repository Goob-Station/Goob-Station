using Content.Goobstation.Shared.Terror.Components;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Content.Shared.StepTrigger.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Terror.Systems;

/// <summary>
/// Makes an entity transparent on top of a tile. Component goes on the tile rather than the person stepping on it.
/// </summary>
public sealed class InvisibleOnTileSystem : EntitySystem
{
    [Dependency] private readonly SharedStealthSystem _stealth = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<InvisibleOnTileComponent, StepTriggeredOnEvent>(OnStepOn);
    }

    private void OnStepOn(EntityUid uid, InvisibleOnTileComponent comp, ref StepTriggeredOnEvent args)
    {
        var tripper = args.Tripper;

        if (!TryComp<TerrorSpiderComponent>(tripper, out var spider))
            return;

        if (!_proto.TryIndex(spider.SpiderType, out var proto))
            return;

        if (!proto.IsInvisibleOnWeb || TerminatingOrDeleted(tripper))
            return;

        var invis = EnsureComp<InvisibleOnTileComponent>(tripper);
        invis.ExpireAt = _timing.CurTime + TimeSpan.FromSeconds(comp.ExpireTime);

        // Apply or refresh stealth
        var stealth = EnsureComp<StealthComponent>(tripper);
        _stealth.SetVisibility(tripper, comp.Invisibility, stealth);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;

        var query = EntityQueryEnumerator<InvisibleOnTileComponent>();

        while (query.MoveNext(out var uid, out var invis))
        {
            if (invis.ExpireAt == null || now < invis.ExpireAt)
                continue;

            invis.ExpireAt = null;

            if (!Deleted(uid))
                RemCompDeferred<StealthComponent>(uid);
        }
    }
}
