using Content.Client.Clothing;
using Content.Client.Items.Systems;
using Content.Pirate.Shared.Stains.Components;
using Content.Pirate.Shared.Stains.Systems;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Clothing;
using Content.Shared.Hands;
using Content.Shared.Humanoid;
using Content.Shared.Inventory;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Client.GameObjects;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;

namespace Content.Pirate.Client.Stains;

public sealed class StainSystem : SharedStainSystem
{
    private const string BloodRsiPath = "_Pirate/Effects/blood.rsi";
    private const string ItemBloodState = "itemblood";
    private const string BareFeetLayerKey = "stain-bare-feet";
    private const string BareHandsLayerKey = "stain-bare-hands";
    private const string StainMaskShaderPrefix = "StainItemMask";
    private const int StainMaskVariants = 8;
    private const string StainMaskTextureParam = "stainMask";
    private const string StainMaskUvParam = "stainMaskUV";

    // Stable per-item mask variation.
    private static string StainShaderFor(EntityUid uid)
    {
        return $"{StainMaskShaderPrefix}{(uid.Id % StainMaskVariants + StainMaskVariants) % StainMaskVariants}";
    }

    [Dependency] private readonly IPrototypeManager _prototypeManager = null!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = null!;
    [Dependency] private readonly SpriteSystem _sprite = null!;
    [Dependency] private readonly ItemSystem _item = null!;

    // Cached across prediction rollbacks.
    private readonly Dictionary<EntityUid, (Color Color, SlotFlags Slots, bool HasStain, string Frame)> _lastDrawn = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StainableComponent, AppearanceChangeEvent>(OnAppearanceChanged);
        SubscribeLocalEvent<StainableComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<StainableComponent, GetEquipmentVisualsEvent>(OnEquipmentVisuals, after: [typeof(ClientClothingSystem)]);
        SubscribeLocalEvent<StainableComponent, GetInhandVisualsEvent>(OnInhandVisuals, after: [typeof(ItemSystem)]);
    }

    private void OnShutdown(Entity<StainableComponent> ent, ref ComponentShutdown args)
    {
        _lastDrawn.Remove(ent.Owner);
    }

    private void OnAppearanceChanged(Entity<StainableComponent> ent, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        var hasStain = TryGetStainColor(ent, out var color);
        var slots = ent.Comp.BodyStainSlots;
        if (args.AppearanceData.TryGetValue(StainVisuals.BodySlots, out var bodySlotsData) && bodySlotsData is SlotFlags bodySlotFlags)
            slots = bodySlotFlags;

        var spriteEnt = new Entity<SpriteComponent?>(ent.Owner, args.Sprite);

        // Include the base frame for dynamic silhouettes.
        var drawn = (color, slots, hasStain, BaseFrameFingerprint(spriteEnt));
        if (_lastDrawn.TryGetValue(ent.Owner, out var last) && last == drawn)
            return;
        _lastDrawn[ent.Owner] = drawn;

        // Appearance data may already match on the client, so notify item visuals directly.
        _item.VisualsChanged(ent.Owner);

        foreach (var key in ent.Comp.RevealedLayerKeys)
        {
            _sprite.RemoveLayer(spriteEnt, key, false);
        }

        ent.Comp.RevealedLayerKeys.Clear();

        var layers = new List<int>(ent.Comp.RevealedLayers);
        layers.Sort((a, b) => b.CompareTo(a));

        foreach (var layer in layers)
        {
            _sprite.RemoveLayer(spriteEnt, layer, false);
        }

        ent.Comp.RevealedLayers.Clear();

        if (!hasStain)
            return;

        var addedPrototypeVisuals = false;
        foreach (var (key, layerData) in BuildVisuals(ent, ent.Comp.IconVisuals, "icon"))
        {
            ent.Comp.RevealedLayerKeys.Add(key);
            _sprite.AddLayer(spriteEnt, layerData, null);
            addedPrototypeVisuals = true;
        }

        if (HasComp<HumanoidAppearanceComponent>(ent.Owner))
            AddBodyStainVisuals(ent, args, spriteEnt, color);
        else if (!addedPrototypeVisuals)
            AddItemBloodIconVisual(ent, spriteEnt, color);
    }

    private void OnEquipmentVisuals(Entity<StainableComponent> ent, ref GetEquipmentVisualsEvent args)
    {
        if (ent.Comp.ClothingVisuals.TryGetValue(args.Slot, out var layers))
            args.Layers.AddRange(BuildVisuals(ent, layers, args.Slot));
    }

    private void OnInhandVisuals(Entity<StainableComponent> ent, ref GetInhandVisualsEvent args)
    {
        if (ent.Comp.ItemVisuals.TryGetValue(args.Location.ToString(), out var layers))
        {
            args.Layers.AddRange(BuildVisuals(ent, layers, args.Location.ToString()));
            return;
        }

        if (!TryGetStainColor(ent, out var color) || args.Layers.Count == 0)
            return;

        // Snapshot before appending stain layers.
        var baseLayers = new List<(string, PrototypeLayerData)>(args.Layers);
        for (var i = 0; i < baseLayers.Count; i++)
        {
            var source = baseLayers[i].Item2;
            if (source.Visible == false)
                continue;

            var drawnKey = $"stain-inhand-{args.Location}-{i}";
            var copyKey = $"stain-inhand-blood-{args.Location}-{i}";

            // Redraw the item's OWN in-hand sprite, shaded to paint blood over its silhouette, so the held
            // sprite's bounds/click map stay the item's. The blood texture comes from the copy layer, added
            // first so it renders (and sets the shader params) before the drawn layer. BaseRSI-only layers are
            // resolved by HandsSystem for the drawn layer the same way it resolves the base layer.
            args.Layers.Add((copyKey, BuildBloodCopyLayer(copyKey, drawnKey, source)));
            args.Layers.Add((drawnKey, BuildStainDrawnLayer(drawnKey, source, color, ent.Owner)));
        }
    }

    // Tracks dynamic icon silhouettes.
    private string BaseFrameFingerprint(Entity<SpriteComponent?> sprite)
    {
        if (HasComp<HumanoidAppearanceComponent>(sprite.Owner))
            return string.Empty;

        // Icon stains mask every visible base layer, so track visible state changes.
        var fingerprint = string.Empty;
        for (var i = 0; _sprite.TryGetLayer(sprite, i, out var layer, false); i++)
        {
            var state = _sprite.LayerGetRsiState(sprite, i);
            fingerprint += $"{(_sprite.IsVisible(layer) ? 1 : 0)}:{(state.IsValid ? state.Name : string.Empty)}|";
        }

        return fingerprint;
    }

    private IEnumerable<(string, PrototypeLayerData)> BuildVisuals(Entity<StainableComponent> ent, List<PrototypeLayerData> templates, string prefix)
    {
        if (!TryGetStainColor(ent, out var color))
            yield break;

        for (var i = 0; i < templates.Count; i++)
        {
            var layer = templates[i];
            var key = $"stain-{prefix}-{i}";
            yield return (key, CopyVisualLayer(layer, color, key));
        }
    }

    private bool TryGetStainColor(Entity<StainableComponent> ent, out Color color)
    {
        color = Color.White;

        if (!_solution.TryGetSolution(ent.Owner, ent.Comp.SolutionName, out _, out var sol) || sol.Volume <= FixedPoint2.Zero)
            return false;

        color = sol.GetColor(_prototypeManager);
        return true;
    }

    private void AddItemBloodIconVisual(Entity<StainableComponent> ent, Entity<SpriteComponent?> sprite, Color color)
    {
        var baseCount = 0;
        while (_sprite.TryGetLayer(sprite, baseCount, out _, false))
            baseCount++;

        for (var i = 0; i < baseCount; i++)
        {
            if (!_sprite.TryGetLayer(sprite, i, out var layer, false) || !_sprite.IsVisible(layer))
                continue;

            // Skip shaded overlay layers (e.g. borg eye/light glows use the 'unshaded' shader). Blood should
            // clip to the solid body/item layers only; masking it onto a full-frame glow layer makes the
            // overlay cover the whole sprite (this is why some borgs showed a full blood square).
            if (layer.ShaderPrototype != null)
                continue;

            var state = _sprite.LayerGetRsiState(sprite, i);
            var rsi = _sprite.LayerGetEffectiveRsi(sprite, i);
            if (rsi == null || !state.IsValid)
                continue;

            var drawnKey = $"stain-icon-{i}";
            var copyKey = $"stain-icon-blood-{i}";

            var itemSource = new PrototypeLayerData { RsiPath = rsi.Path.ToString(), State = state.Name };

            // Blood copy layer first (lower index -> renders first, feeds the shader; CopyToShaderParameters
            // keeps it out of bounds/click maps).
            ent.Comp.RevealedLayerKeys.Add(copyKey);
            _sprite.AddLayer(sprite, BuildBloodCopyLayer(copyKey, drawnKey, itemSource), null);

            // Drawn layer redraws the item's OWN sprite, shaded to paint blood - so the item's click map/bounds
            // are unchanged instead of being inflated by a full-frame blood blob.
            ent.Comp.RevealedLayerKeys.Add(drawnKey);
            _sprite.AddLayer(sprite, BuildStainDrawnLayer(drawnKey, itemSource, color, ent.Owner), null);
        }
        // No maskable base layer means no stain overlay.
    }

    // Copy layer: supplies the blood texture to the drawn layer's shader. CopyToShaderParameters means it is
    // not rendered and is excluded from the sprite's bounding box and (pixel-perfect) click map.
    private static PrototypeLayerData BuildBloodCopyLayer(string copyKey, string targetKey, PrototypeLayerData? source = null)
    {
        return new PrototypeLayerData
        {
            RsiPath = BloodRsiPath,
            State = ItemBloodState,
            Scale = source?.Scale,
            Rotation = source?.Rotation,
            Offset = source?.Offset,
            RenderingStrategy = source?.RenderingStrategy,
            MapKeys = new() { copyKey },
            CopyToShaderParameters = new PrototypeCopyToShaderParameters
            {
                LayerKey = targetKey,
                ParameterTexture = StainMaskTextureParam,
                ParameterUV = StainMaskUvParam,
            }
        };
    }

    // Drawn layer: the item's OWN sprite, shaded to paint blood over its silhouette. Using the item's texture
    // keeps the item's bounding box and pixel-perfect click map unchanged (no full-frame blood blob).
    private static PrototypeLayerData BuildStainDrawnLayer(string key, PrototypeLayerData source, Color color, EntityUid uid)
    {
        return new PrototypeLayerData
        {
            RsiPath = source.RsiPath,
            TexturePath = source.TexturePath,
            State = source.State,
            Scale = source.Scale,
            Rotation = source.Rotation,
            Offset = source.Offset,
            Visible = source.Visible,
            RenderingStrategy = source.RenderingStrategy,
            Shader = StainShaderFor(uid),
            Color = color,
            MapKeys = new() { key }
        };
    }

    private void AddBodyStainVisuals(Entity<StainableComponent> ent, AppearanceChangeEvent args, Entity<SpriteComponent?> sprite, Color color)
    {
        var slots = ent.Comp.BodyStainSlots;
        if (args.AppearanceData.TryGetValue(StainVisuals.BodySlots, out var bodySlots) &&
            bodySlots is SlotFlags bodySlotFlags)
        {
            slots = bodySlotFlags;
        }

        // Put bare-body stains below matching gear so shoes/gloves hide them.
        if ((slots & SlotFlags.FEET) != 0)
            AddBodyStainVisual(ent, sprite, color, BareFeetLayerKey, "shoeblood", "shoes");

        if ((slots & SlotFlags.GLOVES) != 0)
            AddBodyStainVisual(ent, sprite, color, BareHandsLayerKey, "gloveblood", "gloves");
    }

    private void AddBodyStainVisual(Entity<StainableComponent> ent, Entity<SpriteComponent?> sprite, Color color, string key, string state, string slotBookmark)
    {
        var layerData = new PrototypeLayerData
        {
            RsiPath = "_Pirate/Effects/blood.rsi",
            State = state,
            Color = color,
            MapKeys = new() { key }
        };

        ent.Comp.RevealedLayerKeys.Add(key);

        if (_sprite.LayerMapTryGet(sprite, slotBookmark, out var index, false))
            _sprite.AddLayer(sprite, layerData, index);
        else
            _sprite.AddLayer(sprite, layerData, null);
    }

    private static PrototypeLayerData CopyVisualLayer(PrototypeLayerData source, Color color, string key)
    {
        return new PrototypeLayerData
        {
            TexturePath = source.TexturePath,
            RsiPath = source.RsiPath,
            State = source.State,
            Scale = source.Scale,
            Rotation = source.Rotation,
            Offset = source.Offset,
            Visible = source.Visible,
            RenderingStrategy = source.RenderingStrategy,
            Color = color,
            MapKeys = new() { key }
        };
    }
}
