using Content.Shared._Funkystation.Clothing.Components;
using Content.Shared.CCVar;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Client._Funkystation.Clothing.Systems;

public sealed partial class GoggleShaderSystem : EntitySystem
{
    [Dependency] private IOverlayManager _overlayMan = null!;
    [Dependency] private IPlayerManager _playerManager = null!;
    [Dependency] private InventorySystem _inventory = null!;
    [Dependency] private IConfigurationManager _cfg = null!;

    private GoggleShaderOverlay _overlay = null!;

    public override void Initialize()
    {
        base.Initialize();
        _overlay = new GoggleShaderOverlay(ProtoMan);

        _cfg.OnValueChanged(CCVars.ReducedMotion, value => _overlay.ReducedMotion = value, true);

        SubscribeLocalEvent<GoggleShaderComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<GoggleShaderComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<GoggleShaderComponent, GotEquippedEvent>(OnEquip);
        SubscribeLocalEvent<GoggleShaderComponent, GotUnequippedEvent>(OnUnequip);
        SubscribeLocalEvent<GoggleShaderComponent, GoggleShaderToggledEvent>(OnGoggleShaderToggled);
        SubscribeLocalEvent<LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<LocalPlayerDetachedEvent>(OnPlayerDetached);
    }

    private void OnStartup(Entity<GoggleShaderComponent> ent, ref ComponentStartup args)
    {
        RefreshOverlay();
    }

    private void OnShutdown(Entity<GoggleShaderComponent> ent, ref ComponentShutdown args)
    {
        RefreshOverlay(ignoreEnt: ent.Owner);
    }

    private void OnEquip(Entity<GoggleShaderComponent> ent, ref GotEquippedEvent args)
    {
        if (args.Equipee == _playerManager.LocalEntity && (args.Slot == "head" || args.Slot == "eyes"))
            RefreshOverlay();
    }

    private void OnUnequip(Entity<GoggleShaderComponent> ent, ref GotUnequippedEvent args)
    {
        if (args.Equipee == _playerManager.LocalEntity && (args.Slot == "head" || args.Slot == "eyes"))
            RefreshOverlay(ignoreEnt: ent.Owner);
    }

    private void OnGoggleShaderToggled(Entity<GoggleShaderComponent> ent, ref GoggleShaderToggledEvent args)
    {
        RefreshOverlay();
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
        var localPlayer = _playerManager.LocalEntity;

        if (localPlayer == null)
        {
            RemoveOverlay();
            return;
        }

        _overlay.ActiveShaders.Clear();

        if (_inventory.TryGetSlotEntity(localPlayer.Value, "eyes", out var eyesItem) &&
            eyesItem != ignoreEnt &&
            TryComp<GoggleShaderComponent>(eyesItem.Value, out var eyesComp) &&
            eyesComp.Enabled)
        {
            _overlay.ActiveShaders.Add((eyesComp.Shader, eyesComp.Color));
        }

        if (_inventory.TryGetSlotEntity(localPlayer.Value, "head", out var headItem) &&
            headItem != ignoreEnt &&
            TryComp<GoggleShaderComponent>(headItem.Value, out var headComp) &&
            headComp.Enabled)
        {
            _overlay.ActiveShaders.Add((headComp.Shader, headComp.Color));
        }

        if (_overlay.ActiveShaders.Count > 0)
        {
            AddOverlay();
        }
        else
        {
            RemoveOverlay();
        }
    }

    private void AddOverlay()
    {
        if (!_overlayMan.HasOverlay<GoggleShaderOverlay>())
            _overlayMan.AddOverlay(_overlay);
    }

    private void RemoveOverlay()
    {
        _overlay.ActiveShaders.Clear();
        if (_overlayMan.HasOverlay<GoggleShaderOverlay>())
            _overlayMan.RemoveOverlay(_overlay);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        RemoveOverlay();
    }
}
