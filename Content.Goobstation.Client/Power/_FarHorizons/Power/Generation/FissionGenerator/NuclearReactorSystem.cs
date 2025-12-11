using Robust.Shared.Prototypes;
using Robust.Shared.Map;
using Content.Shared._FarHorizons.Power.Generation.FissionGenerator;
using Content.Client.Examine;
using Robust.Client.GameObjects;
using Robust.Client.ResourceManagement;

namespace Content.Client._FarHorizons.Power.Generation.FissionGenerator;

public sealed class NuclearReactorSystem : SharedNuclearReactorSystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly IResourceCache _resourceCache = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NuclearReactorComponent, ComponentInit>(OnInit);

        SubscribeLocalEvent<NuclearReactorComponent, ClientExaminedEvent>(ReactorExamined);
        SubscribeLocalEvent<NuclearReactorComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnInit(EntityUid uid, NuclearReactorComponent comp, ref ComponentInit args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (!_resourceCache.TryGetResource("/Textures/_FarHorizons/Structures/Power/Generation/FissionGenerator/reactor_component_cap.rsi", out RSIResource? resource))
            return;

        Entity<SpriteComponent?> entSprite = (uid, sprite);
        var xspace = comp.Gridbounds[0] / 32f;
        var yspace = comp.Gridbounds[1] / 32f;
        var xoff = comp.Gridbounds[2] / 32f;
        var yoff = comp.Gridbounds[3] / 32f;

        var gridWidth = comp.ReactorGridWidth;
        var gridHeight = comp.ReactorGridHeight;

        var xAdj = (gridWidth - 1) / 2f;
        var yAdj = (gridHeight - 1) / 2f;

        for (var x = 0; x < gridWidth; x++)
        {
            for (var y = 0; y < gridHeight; y++)
            {
                var layerID = _sprite.AddRsiLayer(entSprite, "empty_cap", resource.RSI);
                _sprite.LayerMapSet(entSprite, FormatMap(x, y), layerID);
                _sprite.LayerSetOffset(entSprite, layerID, new((xspace * (y - yAdj)) - xoff, (-yspace * (x - xAdj)) - yoff));
                _sprite.LayerSetColor(entSprite, layerID, Color.Black);
            }
        }
    }

    private static string FormatMap(int x, int y) => "NuclearReactorCap" + x + "/" + y;

    private void ReactorExamined(EntityUid uid, NuclearReactorComponent comp, ClientExaminedEvent args) => Spawn(comp.ArrowPrototype, new EntityCoordinates(uid, 0, 0));

    private void OnAppearanceChange(EntityUid uid, NuclearReactorComponent comp, ref AppearanceChangeEvent args)
    {
        for (var x = 0; x < comp.ReactorGridWidth; x++)
        {
            for (var y = 0; y < comp.ReactorGridHeight; y++)
            {
                if(comp.VisualData.TryGetValue(new(x,y), out var data))
                    UpdateRodAppearance(uid, FormatMap(x,y), data.cap, data.color);
                else
                    UpdateRodAppearance(uid, FormatMap(x, y), "empty_cap", Color.Black);
            }
        }
    }

    private void UpdateRodAppearance(EntityUid uid, string map, string state, Color color)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        Entity<SpriteComponent?> entSprite = (uid, sprite);

        if (!_sprite.LayerMapTryGet(entSprite, map, out var layer, false))
            return;

        _sprite.LayerSetRsiState(entSprite, layer, state);
        _sprite.LayerSetColor(entSprite, layer, color);
    }
}