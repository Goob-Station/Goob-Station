using Content.Shared._Goobstation.Wizard.Traps;
using Robust.Client.GameObjects;

namespace Content.Client._Goobstation.Wizard.Systems;

public sealed class IceCubeSystem : SharedIceCubeSystem
{
    protected override void Shutdown(Entity<IceCubeComponent> ent)
    {
        var (uid, _) = ent;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (!sprite.LayerMapTryGet(IceCubeKey.Key, out var layer))
            return;

        sprite.RemoveLayer(layer);
    }

    protected override void Startup(Entity<IceCubeComponent> ent)
    {
        var (uid, comp) = ent;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (sprite.LayerMapTryGet(IceCubeKey.Key, out _))
            return;

        var layer = sprite.AddLayer(comp.Sprite);
        sprite.LayerMapSet(IceCubeKey.Key, layer);
    }
}
