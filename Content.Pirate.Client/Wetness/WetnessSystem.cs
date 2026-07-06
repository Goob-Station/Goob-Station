using Content.Pirate.Shared.Wetness.Components;
using Content.Pirate.Shared.Wetness.Systems;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Pirate.Client.Wetness;

/// <summary>
/// Draws the worn-clothing droplet overlay.
/// </summary>
public sealed class WetnessSystem : SharedWetnessSystem
{
    private const string DropletRsi = "_Pirate/Effects/wetness.rsi";
    private const string DropletState = "droplets";
    private const string DropletLayerKey = "wetness-droplets";

    [Dependency] private readonly SpriteSystem _sprite = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WetVisualsComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<WetVisualsComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<WetVisualsComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(ent.Owner, out var sprite))
            return;

        var spriteEnt = new Entity<SpriteComponent?>(ent.Owner, sprite);
        if (_sprite.LayerMapTryGet(spriteEnt, DropletLayerKey, out _, false))
            return;

        _sprite.AddLayer(spriteEnt, new PrototypeLayerData
        {
            RsiPath = DropletRsi,
            State = DropletState,
            MapKeys = new() { DropletLayerKey }
        }, null);
    }

    private void OnShutdown(Entity<WetVisualsComponent> ent, ref ComponentShutdown args)
    {
        if (TryComp<SpriteComponent>(ent.Owner, out var sprite))
            _sprite.RemoveLayer(new Entity<SpriteComponent?>(ent.Owner, sprite), DropletLayerKey, false);
    }
}
