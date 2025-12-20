using Content.Shared.StatusEffectNew;

namespace Content.Goobstation.Server.ComponentsRegistry;
public sealed partial class GrantComponentsStatusEffectSystem : EntitySystem
{
    // please don't use it for anything more complicated than adding immunity to stuff.
    // even so this could potentially break so much shit.

    // but it's more convenient than adding 100 bajillion of bloat status effects that inflate the project's filesize like a balloon

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GrantComponentsStatusEffectComponent, StatusEffectAppliedEvent>(OnStatusEffectApply);
        SubscribeLocalEvent<GrantComponentsStatusEffectComponent, StatusEffectRemovedEvent>(OnStatusEffectRemove);
    }

    private void OnStatusEffectApply(Entity<GrantComponentsStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        EntityManager.AddComponents(args.Target, ent.Comp.Components);
    }

    private void OnStatusEffectRemove(Entity<GrantComponentsStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        EntityManager.RemoveComponents(args.Target, ent.Comp.Components);
    }
}
