using Content.Goobstation.Shared.Hamon;
using Content.Goobstation.Shared.Hamon.Components;
using Robust.Client.GameObjects;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.Hamon;

public sealed class ClientHamonSystem : HamonSystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HamonInfusedComponent, ComponentStartup>(InfusedStartup);
        SubscribeLocalEvent<HamonInfusedComponent, ComponentShutdown>(InfusedShutDown);
    }

    private void InfusedStartup(Entity<HamonInfusedComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(ent.Owner, out var sprite))
            return;

        var layer = _sprite.AddLayer((ent, sprite), new SpriteSpecifier.Rsi(ent.Comp.RsiPath, ent.Comp.RsiState));
        _sprite.LayerMapSet((ent, sprite), HamonInfusedVisuals.Key, layer);
    }

    private void InfusedShutDown(Entity<HamonInfusedComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(ent.Owner, out var sprite))
            return;

        _sprite.RemoveLayer((ent.Owner, sprite), HamonInfusedVisuals.Key);
    }
}

public enum HamonInfusedVisuals : byte
{
    Key,
}
