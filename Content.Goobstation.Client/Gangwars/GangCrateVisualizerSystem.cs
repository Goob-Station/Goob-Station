using Content.Goobstation.Shared.Gangwars.Components;
using Robust.Client.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.Gangwars;

/// <summary>
/// Manages the overlay light layer on the gang crate sprite.
/// Starts at full light when spawned and progresses to blinking as the anchor timer expires.
/// </summary>
public sealed class GangCrateVisualizerSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    private static readonly ResPath LightRsi = new("_Goobstation/Gangs/Crates/crate_light.rsi");

    private static readonly string[] LightStates =
    [
        "off",
        "blink",
        "low_light",
        "half_light",
        "full_light",
    ];

    private enum GangCrateLayers
    {
        Light,
    }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GangCrateComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<GangCrateComponent, AfterAutoHandleStateEvent>(OnStateUpdate);
    }

    private void OnStartup(EntityUid uid, GangCrateComponent comp, ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        var isOff = comp.LightState == GangCrateLightState.Off;
        _sprite.LayerMapReserve((uid, sprite), GangCrateLayers.Light);
        _sprite.LayerSetRsi((uid, sprite), GangCrateLayers.Light, LightRsi, isOff ? LightStates[(int) GangCrateLightState.Blinking] : LightStates[(int) comp.LightState]);
        sprite.LayerSetShader(GangCrateLayers.Light, "unshaded");
        _sprite.LayerSetVisible((uid, sprite), GangCrateLayers.Light, !isOff);
    }

    private void OnStateUpdate(EntityUid uid, GangCrateComponent comp, ref AfterAutoHandleStateEvent args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite)
            || !_sprite.LayerMapTryGet((uid, sprite), GangCrateLayers.Light, out var layerIdx, false))
            return;

        var isOff = comp.LightState == GangCrateLightState.Off;
        _sprite.LayerSetVisible((uid, sprite), layerIdx, !isOff);
        if (!isOff)
            _sprite.LayerSetRsiState((uid, sprite), layerIdx, LightStates[(int) comp.LightState]);
    }
}
