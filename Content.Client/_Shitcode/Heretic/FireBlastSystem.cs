using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Systems;
using Robust.Client.GameObjects;

namespace Content.Client._Shitcode.Heretic;

public sealed class FireBlastSystem : SharedFireBlastSystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FireBlastedComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<FireBlastedComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnShutdown(Entity<FireBlastedComponent> ent, ref ComponentShutdown args)
    {
        var (uid, _) = ent;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (!_sprite.LayerMapTryGet((uid, sprite), FireBlastedKey.Key, out var layer, false))
            return;

        _sprite.RemoveLayer((uid, sprite), layer);
    }

    private void OnStartup(Entity<FireBlastedComponent> ent, ref ComponentStartup args)
    {
        var (uid, comp) = ent;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (_sprite.LayerMapTryGet((uid, sprite), FireBlastedKey.Key, out _, false))
            return;

        var layer = _sprite.AddLayer((uid, sprite), comp.Sprite);

        _sprite.LayerMapSet((uid, sprite), FireBlastedKey.Key, layer);
    }
}
