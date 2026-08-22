using Content.Shared._Shitmed.Body.Organ;
using Content.Shared._Shitmed.Medical.Surgery.Traumas;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared.Body.Part;

namespace Content.Server._Shitmed.Medical.Trauma;

public sealed class BraindeathSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BraindeathComponent, TraumaInducedEvent>(OnInduced);
        SubscribeLocalEvent<BraindeathComponent, TraumaBeingRemovedEvent>(OnRemoved);
    }

    private void OnInduced(Entity<BraindeathComponent> ent, ref TraumaInducedEvent args)
    {
        if (TryGetBody(args.Trauma.Comp.HoldingWoundable, out var body))
            EnsureComp<DebrainedComponent>(body);
    }

    private void OnRemoved(Entity<BraindeathComponent> ent, ref TraumaBeingRemovedEvent args)
    {
        if (TryGetBody(args.Trauma.Comp.HoldingWoundable, out var body))
            RemComp<DebrainedComponent>(body);
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
}
