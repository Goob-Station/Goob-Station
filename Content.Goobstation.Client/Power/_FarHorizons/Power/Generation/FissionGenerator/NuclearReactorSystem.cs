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
    [Dependency] private readonly IPrototypeManager _proto = default!;

    private static readonly EntProtoId ArrowPrototype = "ReactorFlowArrow";

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
        var xspace = 18f / 32f;
        var yspace = 15f / 32f;
        var yoff = 5f / 32f;
        var prefab = SelectPrefab(comp.Prefab);

        for (var x = 0; x < _gridWidth; x++)
        {
            for (var y = 0; y < _gridHeight; y++)
            {
                var state = prefab[x, y] != null ? prefab[x, y]!.IconStateCap : "empty_cap";
                var color = prefab[x, y] != null ? _proto.Index(prefab[x, y]!.Material).Color : Color.Black;
                var layerID = _sprite.AddRsiLayer(entSprite, state, resource.RSI);
                _sprite.LayerMapSet(entSprite, FormatMap(x, y), layerID);
                _sprite.LayerSetOffset(entSprite, layerID, new(xspace * (y - 3), (-yspace * (x - 3)) - yoff));
                _sprite.LayerSetColor(entSprite, layerID, color);
            }
        }
    }

    private static string FormatMap(int x, int y) => "NuclearReactorCap" + x + "/" + y;

    private void ReactorExamined(EntityUid uid, NuclearReactorComponent comp, ClientExaminedEvent args) => Spawn(ArrowPrototype, new EntityCoordinates(uid, 0, 0));

    private void OnAppearanceChange(Entity<NuclearReactorComponent> ent, ref AppearanceChangeEvent args)
    {
        for (var x = 0; x < _gridWidth; x++)
        {
            for (var y = 0; y < _gridHeight; y++)
            {
                if(ent.Comp.VisualData.TryGetValue(new(x,y), out var data))
                    UpdateRodAppearance(ent.Owner, FormatMap(x,y), data.cap, data.color);
                else
                    UpdateRodAppearance(ent.Owner, FormatMap(x, y), "empty_cap", Color.Black);
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