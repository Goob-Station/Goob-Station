using Content.Shared._Funkystation.Clothing.Components;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Shared.Player;

namespace Content.Client._Funkystation.Clothing.Systems;

public sealed partial class WeldingMaskOverlaySystem : EntitySystem
{
    [Dependency] private IPlayerManager _player = null!;
    [Dependency] private IOverlayManager _overlayMan = null!;
    [Dependency] private IResourceCache _cache = null!;
    [Dependency] private InventorySystem _inventory = null!;

    private WeldingMaskOverlay _overlay = null!;

    public override void Initialize()
    {
        base.Initialize();

        _overlay = new WeldingMaskOverlay(_cache);

        SubscribeLocalEvent<WeldingMaskOverlayComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<WeldingMaskOverlayComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<WeldingMaskOverlayComponent, GotEquippedEvent>(OnEquip);
        SubscribeLocalEvent<WeldingMaskOverlayComponent, GotUnequippedEvent>(OnUnequip);
        SubscribeLocalEvent<LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<LocalPlayerDetachedEvent>(OnPlayerDetached);
    }

    private void OnStartup(Entity<WeldingMaskOverlayComponent> ent, ref ComponentStartup args)
    {
        RefreshOverlay();
    }

    private void OnShutdown(Entity<WeldingMaskOverlayComponent> ent, ref ComponentShutdown args)
    {

        RefreshOverlay(ignoreEnt: ent.Owner);
    }

    private void OnEquip(Entity<WeldingMaskOverlayComponent> ent, ref GotEquippedEvent args)
    {

        if (args.Equipee == _player.LocalSession?.AttachedEntity && (args.Slot == "head" || args.Slot == "mask"))
            RefreshOverlay();
    }

    private void OnUnequip(Entity<WeldingMaskOverlayComponent> ent, ref GotUnequippedEvent args)
    {
        if (args.Equipee == _player.LocalSession?.AttachedEntity && (args.Slot == "head" || args.Slot == "mask"))
            RefreshOverlay(ignoreEnt: ent.Owner);
    }

    private void OnPlayerAttached(LocalPlayerAttachedEvent args)
    {
        RefreshOverlay();
    }

    private void OnPlayerDetached(LocalPlayerDetachedEvent args)
    {
        RefreshOverlay();
    }

    private void RefreshOverlay(EntityUid? ignoreEnt = null)
    {
        var localPlayer = _player.LocalSession?.AttachedEntity;

        if (localPlayer == null)
        {
            RemoveOverlay();
            return;
        }

        if (_inventory.TryGetSlotEntity(localPlayer.Value, "head", out var headItem) &&
            headItem != ignoreEnt &&
            TryComp<WeldingMaskOverlayComponent>(headItem.Value, out var headComp))
        {
            _overlay.CurrentTexturePath = headComp.Texture;
            AddOverlay();
            return;
        }

        if (_inventory.TryGetSlotEntity(localPlayer.Value, "mask", out var maskItem) &&
            maskItem != ignoreEnt &&
            TryComp<WeldingMaskOverlayComponent>(maskItem.Value, out var maskComp))
        {
            _overlay.CurrentTexturePath = maskComp.Texture;
            AddOverlay();
            return;
        }

        RemoveOverlay();
    }

    private void AddOverlay()
    {
        if (!_overlayMan.HasOverlay<WeldingMaskOverlay>())
            _overlayMan.AddOverlay(_overlay);
    }

    private void RemoveOverlay()
    {
        if (_overlayMan.HasOverlay<WeldingMaskOverlay>())
            _overlayMan.RemoveOverlay(_overlay);
    }
}
