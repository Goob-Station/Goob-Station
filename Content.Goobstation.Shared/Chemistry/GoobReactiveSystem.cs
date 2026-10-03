using Content.Goobstation.Common.Chemistry;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.EntityEffects;

namespace Content.Goobstation.Shared.Chemistry;

public sealed partial class GoobReactiveSystem : EntitySystem
{
    [Dependency] private readonly SharedEntityEffectsSystem _effect = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ReactiveComponent, ReactionEntityDoneEvent>(OnReactionEntityDone);
    }

    private void OnReactionEntityDone(Entity<ReactiveComponent> ent, ref ReactionEntityDoneEvent args)
    {
        if (ent.Comp.Effects == null)
            return;

        _effect.ApplyEffects(ent.Owner, ent.Comp.Effects);
    }
}
