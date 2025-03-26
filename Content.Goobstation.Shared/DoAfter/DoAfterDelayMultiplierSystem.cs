using Content.Goobstation.Common.DoAfter;
using Content.Shared._Shitmed.Cybernetics;
using Content.Shared._Shitmed.DoAfter;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;

namespace Content.Goobstation.Shared.DoAfter;

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
