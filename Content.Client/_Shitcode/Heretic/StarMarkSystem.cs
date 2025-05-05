using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Systems;
using Robust.Client.GameObjects;

namespace Content.Client._Shitcode.Heretic;

public sealed class StarMarkSystem : SharedStarMarkSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StarMarkComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<StarMarkComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnShutdown(Entity<StarMarkComponent> ent, ref ComponentShutdown args)
    {
        var (uid, _) = ent;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (!sprite.LayerMapTryGet(StarMarkKey.Key, out var layer))
            return;

        sprite.RemoveLayer(layer);
    }

    private void OnStartup(Entity<StarMarkComponent> ent, ref ComponentStartup args)
    {
        var (uid, comp) = ent;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (sprite.LayerMapTryGet(StarMarkKey.Key, out _))
            return;

        var layer = sprite.AddLayer(comp.Sprite);
        sprite.LayerSetShader(layer, "unshaded");
        sprite.LayerMapSet(StarMarkKey.Key, layer);
    }
}
