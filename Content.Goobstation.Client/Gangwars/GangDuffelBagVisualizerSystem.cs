using Content.Client.Storage.Visualizers;
using Content.Goobstation.Shared.Gangwars.Components;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.Gangwars;

public sealed class GangDuffelBagVisualizerSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    private static readonly string[] StateNames =
    [
        "icon_trapped",
        "icon",
        "icon_open",
    ];

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GangDuffelBagComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<GangDuffelBagComponent, AfterAutoHandleStateEvent>(OnStateUpdate);
    }

    private void OnStartup(EntityUid uid, GangDuffelBagComponent comp, ComponentStartup args) =>
        UpdateSprite(uid, comp);

    private void OnStateUpdate(EntityUid uid, GangDuffelBagComponent comp, ref AfterAutoHandleStateEvent args) =>
        UpdateSprite(uid, comp);

    private void UpdateSprite(EntityUid uid, GangDuffelBagComponent comp)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        var state = StateNames[(int) comp.State];

        if (_sprite.LayerMapTryGet((uid, sprite), StorageVisualLayers.Base, out var idx, false))
            _sprite.LayerSetRsiState((uid, sprite), idx, state);
    }
}

