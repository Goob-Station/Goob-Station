using Content.Shared.StatusEffectNew;
using Robust.Shared.Map;

namespace Content.Shared.Heretic.Effects;

public sealed class CrucibleSoulStatusEffectSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CrucibleSoulStatusEffectComponent, StatusEffectAppliedEvent>(OnApply);
        SubscribeLocalEvent<CrucibleSoulStatusEffectComponent, StatusEffectRemovedEvent>(OnRemove);

        SubscribeLocalEvent<CrucibleSoulRecallEvent>(OnRecall);
    }

    private void OnRecall(CrucibleSoulRecallEvent ev)
    {
        _status.TryRemoveStatusEffect(ev.User, ev.EffectProto);
    }

    private void OnRemove(Entity<CrucibleSoulStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        if (ent.Comp.Coords == null || TerminatingOrDeleted(args.Target))
            return;

        _transform.SetCoordinates(args.Target, ent.Comp.Coords.Value);
        _transform.AttachToGridOrMap(args.Target);
    }

    private void OnApply(Entity<CrucibleSoulStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        ent.Comp.Coords = Transform(args.Target).Coordinates;
    }
}
