using Content.Goobstation.Shared.Medical;
using Content.Goobstation.Shared.Medical.Components;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.Medical;

public sealed class MedigunSystem : SharedMedigunSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MediGunHealedComponent, ComponentStartup>(OnMedigunHealed);
        SubscribeLocalEvent<MediGunHealedComponent, ComponentShutdown>(OnMedigunShutdown);
        SubscribeLocalEvent<MediGunHealedComponent, AfterAutoHandleStateEvent>(OnColorChanged);
    }

    private void OnColorChanged(Entity<MediGunHealedComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
        {
            return;
        }

        sprite.Color = ent.Comp.LineColor;
    }

    private void OnMedigunHealed(Entity<MediGunHealedComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
        {
            return;
        }

        sprite.Color = ent.Comp.LineColor;
    }

    private void OnMedigunShutdown(Entity<MediGunHealedComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
        {
            return;
        }

        sprite.Color = Color.White;
    }
}
