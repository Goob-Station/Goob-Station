using Content.Shared._Shitmed.Cybernetics;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;

namespace Content.Shared._Goobstation.DoAfter;

public sealed class DoAfterDelayMultiplierSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DoAfterDelayMultiplierComponent, GetDoAfterDelayMultiplierEvent>(OnGetMultiplier);
        SubscribeLocalEvent<DoAfterDelayMultiplierComponent, BodyPartRelayedEvent<GetDoAfterDelayMultiplierEvent>>(
            OnGetBodyPartMultiplier);
    }

    private void OnGetBodyPartMultiplier(Entity<DoAfterDelayMultiplierComponent> ent,
        ref BodyPartRelayedEvent<GetDoAfterDelayMultiplierEvent> args)
    {
        if (TryComp(ent, out CyberneticsComponent? cybernetics) && cybernetics.Disabled)
            args.Args.Multiplier *= 10f;

        args.Args.Multiplier *= ent.Comp.Multiplier;
    }

    private void OnGetMultiplier(Entity<DoAfterDelayMultiplierComponent> ent, ref GetDoAfterDelayMultiplierEvent args)
    {
        args.Multiplier *= ent.Comp.Multiplier;
    }
}

public sealed class GetDoAfterDelayMultiplierEvent(float multiplier = 1f) : EntityEventArgs, IBodyPartRelayEvent
{
    public float Multiplier = multiplier;

    public BodyPartType TargetBodyPart => BodyPartType.Hand;
}
