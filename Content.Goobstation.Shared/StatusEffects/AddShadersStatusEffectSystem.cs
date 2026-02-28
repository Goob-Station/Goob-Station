using Content.Goobstation.Common.Shaders;
using Content.Shared.StatusEffectNew;

namespace Content.Goobstation.Shared.StatusEffects;

public sealed class AddShadersStatusEffectSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AddShadersStatusEffectComponent, StatusEffectAppliedEvent>(OnApply);
        SubscribeLocalEvent<AddShadersStatusEffectComponent, StatusEffectRemovedEvent>(OnRemove);
    }

    private void OnApply(Entity<AddShadersStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        var ev = new SetMultiShadersEvent(ent.Comp.PostShaders, true);
        RaiseLocalEvent(args.Target, ref ev);
    }

    private void OnRemove(Entity<AddShadersStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        var ev = new SetMultiShadersEvent(ent.Comp.PostShaders, false);
        RaiseLocalEvent(args.Target, ref ev);
    }
}
