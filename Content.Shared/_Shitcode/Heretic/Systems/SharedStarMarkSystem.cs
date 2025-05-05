using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Physics.Events;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitcode.Heretic.Systems;

public abstract class SharedStarMarkSystem : EntitySystem
{
    public static readonly EntProtoId StarMarkStatusEffect = "StatusEffectStarMark";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicFieldComponent, PreventCollideEvent>(OnPreventColliede);

        SubscribeLocalEvent<StarMarkStatusEffectComponent, StatusEffectAppliedEvent>(OnApply);
        SubscribeLocalEvent<StarMarkStatusEffectComponent, StatusEffectRemovedEvent>(OnRemove);
    }

    private void OnRemove(Entity<StarMarkStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        if (!TerminatingOrDeleted(args.Target) && TryComp(args.Target, out StarMarkComponent? mark))
            RemCompDeferred(args.Target, mark);
    }

    private void OnApply(Entity<StarMarkStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        EnsureComp<StarMarkComponent>(args.Target);
    }

    private void OnPreventColliede(Entity<CosmicFieldComponent> ent, ref PreventCollideEvent args)
    {
        if (!HasComp<StarMarkComponent>(args.OtherEntity))
            args.Cancelled = true;
    }
}
