using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Goobstation.Heretic.Systems;
using Robust.Client.GameObjects;

namespace Content.Client._Shitcode.Heretic;

public sealed class EntropicPlumeSystem : SharedEntropicPlumeSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EntropicPlumeAffectedComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<EntropicPlumeAffectedComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnShutdown(Entity<EntropicPlumeAffectedComponent> ent, ref ComponentShutdown args)
    {
        var (uid, _) = ent;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (!sprite.LayerMapTryGet(EntropicPlumeKey.Key, out var layer))
            return;

        sprite.RemoveLayer(layer);
    }

    private void OnStartup(Entity<EntropicPlumeAffectedComponent> ent, ref ComponentStartup args)
    {
        var (uid, comp) = ent;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (sprite.LayerMapTryGet(EntropicPlumeKey.Key, out _))
            return;

        var layer = sprite.AddLayer(comp.Sprite);
        sprite.LayerMapSet(EntropicPlumeKey.Key, layer);
    }
}
