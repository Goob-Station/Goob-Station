using Content.Client.Hands;
using Content.Shared.Containers.ItemSlots;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Shared.Containers;
using Robust.Shared.Enums;
using Robust.Shared.Graphics;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Reflection;
using Robust.Shared.Timing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Content.Client._White.ItemSlotRenderer;

/// <summary>
/// I can feel my grip on reality slowly slipping.
/// </summary>
public sealed class ItemSlotRendererSystem : EntitySystem
{
    [Dependency] private readonly IReflectionManager _reflection = default!;
    [Dependency] private readonly ItemSlotsSystem _slot = default!;
    [Dependency] private readonly IClyde _clyde = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ItemSlotRendererComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ItemSlotRendererComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<ItemSlotRendererComponent, EntInsertedIntoContainerMessage>(OnInsertIntoContainer);
        SubscribeLocalEvent<ItemSlotRendererComponent, EntRemovedFromContainerMessage>(OnRemoveFromContainer);

    }
    private void OnInsertIntoContainer(EntityUid uid, ItemSlotRendererComponent comp, EntInsertedIntoContainerMessage args)
    {
        if (args.Container is not ContainerSlot || !_timing.IsFirstTimePredicted)
            return;

        comp.CachedEntities[args.Container.ID] = args.Entity;
    }

    private void OnRemoveFromContainer(EntityUid uid, ItemSlotRendererComponent comp, EntRemovedFromContainerMessage args)
    {
        if (args.Container is not ContainerSlot || !_timing.IsFirstTimePredicted)
            return;

        comp.CachedEntities[args.Container.ID] = null;
    }

    private void OnRemove(EntityUid uid, ItemSlotRendererComponent comp, ComponentRemove args)
    {
        foreach (var (_, renderTexture) in comp.CachedRT)
            renderTexture.Dispose();
    }

    private void OnStartup(EntityUid uid, ItemSlotRendererComponent comp, ComponentStartup args)
    {
        if(!TryComp<SpriteComponent>(uid, out var sprite))
        {
            Log.Error($"ItemSlotRendererCompontn requires SpriteComponent to work, but {ToPrettyString(uid)} did not have one. Removing ItemSlotRenderer.");
            RemComp<ItemSlotRendererComponent>(uid);
            return;
        }

        foreach (var kvp in comp.PrototypeLayerMappings)
        {

            (string slotId, object mapKey) = kvp;
            bool isEnum = false;
            if (_reflection.TryParseEnumReference((string)mapKey, out var e))
            {
                mapKey = e;
                isEnum = true;
            }
            if (!sprite.LayerMapTryGet(mapKey, out _) && comp.ErrorOnMissing)
            {
                Log.Warning($"ItemSlotRenderer: Tried to add a missing layer under the {(isEnum ? "enum" : "string")} key {mapKey}. Skipping missing layer. If this is unwanted, set component's ErrorOnMissing to false.");
                continue;
            }

            if(_slot.TryGetSlot(uid, slotId, out var slot))
                comp.CachedEntities[slotId] = slot.Item;

            comp.LayerMappings.Add((mapKey, slotId));

            comp.CachedRT.Add(slotId, _clyde.CreateRenderTarget(comp.RenderTargetSize, new RenderTargetFormatParameters(RenderTargetColorFormat.Rgba8Srgb), new TextureSampleParameters { Filter = false }, $"{slotId}-itemrender-rendertarget"));
        }
    }
}

/// <summary>
/// Doesn't actually render anything by itself. I'd place this code in a system's FrameUpdate,
/// but I need to somehow acquire a draw handle to draw an entity to a texture.
/// </summary>
public sealed class SpriteToLayerBullshitOverlay : Overlay
{
    [Dependency] private readonly EntityManager _entMan = default!;

    public override OverlaySpace Space => OverlaySpace.ScreenSpaceBelowWorld;

    public SpriteToLayerBullshitOverlay()
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.ScreenHandle;
        var query = _entMan.EntityQueryEnumerator<ItemSlotRendererComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var comp, out var sprite))
        {
            for (int i = 0; i < comp.LayerMappings.Count; i++)
            {
                var (layerKey, slotId) = comp.LayerMappings[i];
                if (!sprite.LayerMapTryGet(layerKey, out int layerIndex) ||
                    !sprite.TryGetLayer(layerIndex, out var layer)) // verify that the layer actually exists
                    continue;

                // if for some reason we can't render the item to a texture (or there is no item to render),
                // assign an "empty" texture to the layer
                if (!comp.CachedEntities.TryGetValue(slotId, out var _item) || _item is not EntityUid item ||
                    !comp.CachedRT.TryGetValue(slotId, out var renderTarget))
                {
                    if (layer.Texture != Texture.Transparent)
                        sprite.LayerSetTexture(layerIndex, Texture.Transparent);
                    continue;
                }

                handle.RenderInRenderTarget(renderTarget, () =>
                {
                    handle.DrawEntity(item, renderTarget.Size / 2, Vector2.One, 0); // If this throws due to a missing spritecomp, it's your fault.
                }, Color.Transparent);
                sprite.LayerSetTexture(layerIndex, renderTarget.Texture);
            }
        }
    }
}
