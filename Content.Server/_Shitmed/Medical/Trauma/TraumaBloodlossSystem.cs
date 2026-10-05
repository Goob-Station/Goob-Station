using Content.Server.Body.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Traumas;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Robust.Shared.Timing;

namespace Content.Server._Shitmed.Medical.Trauma;

public sealed class TraumaBloodlossSystem : EntitySystem
{
    [Dependency] private readonly BloodstreamSystem _blood = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private static readonly TimeSpan UpdateInterval = TimeSpan.FromSeconds(1);
    private TimeSpan _nextUpdate;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TraumaBloodlossComponent, TraumaInducedEvent>(OnInduced);
        SubscribeLocalEvent<TraumaBloodlossComponent, TraumaBeingRemovedEvent>(OnRemoved);
    }

    private void OnInduced(Entity<TraumaBloodlossComponent> ent, ref TraumaInducedEvent args)
    {
        if (!TryGetBody(args.Trauma.Comp.HoldingWoundable, out var body))
            return;

        var bleed = EnsureComp<ConstantBleedComponent>(body);
        bleed.Amount += ent.Comp.Amount;
    }

    private void OnRemoved(Entity<TraumaBloodlossComponent> ent, ref TraumaBeingRemovedEvent args)
    {
        if (!TryGetBody(args.Trauma.Comp.HoldingWoundable, out var body)
            || !TryComp<ConstantBleedComponent>(body, out var bleed))
            return;

        bleed.Amount -= ent.Comp.Amount;
        if (bleed.Amount <= 0)
            RemComp<ConstantBleedComponent>(body);
    }

    private bool TryGetBody(EntityUid? woundable, out EntityUid body)
    {
        body = default;
        if (woundable == null
            || !TryComp<BodyPartComponent>(woundable, out var part)
            || part.Body == null)
            return false;

        body = part.Body.Value;
        return true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_timing.CurTime < _nextUpdate)
            return;

        _nextUpdate = _timing.CurTime + UpdateInterval;

        var query = EntityQueryEnumerator<ConstantBleedComponent, BloodstreamComponent>();
        while (query.MoveNext(out var uid, out var bleed, out var bloodstream))
        {
            var deficit = bleed.Amount - bloodstream.BleedAmountNotFromWounds;
            if (deficit > 0)
                _blood.TryModifyBleedAmount((uid, bloodstream), deficit);
        }
    }
}
