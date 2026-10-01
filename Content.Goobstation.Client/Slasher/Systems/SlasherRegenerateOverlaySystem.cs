using Content.Goobstation.Client.Slasher.Overlays;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Shared._DV.CCVars;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.Slasher.Systems;

/// <summary>
/// Plays the SlasherRegenerateOverlay.
/// </summary>
public sealed class SlasherRegenerateOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private readonly SlasherRegenerateOverlay _overlay = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherRegenerateOverlayComponent, ComponentInit>(OnRegenerateOverlayInit);
        SubscribeLocalEvent<SlasherRegenerateOverlayComponent, ComponentShutdown>(OnRegenerateOverlayShutdown);
        SubscribeLocalEvent<SlasherRegenerateOverlayComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<SlasherRegenerateOverlayComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        Subs.CVar(_cfg, DCCVars.NoVisionFilters, OnNoVisionFiltersChanged);
    }

    private void OnRegenerateOverlayInit(Entity<SlasherRegenerateOverlayComponent> ent, ref ComponentInit args)
    {
        if (ent.Owner == _player.LocalEntity && !_cfg.GetCVar(DCCVars.NoVisionFilters))
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnRegenerateOverlayShutdown(Entity<SlasherRegenerateOverlayComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Owner == _player.LocalEntity)
            _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnPlayerAttached(Entity<SlasherRegenerateOverlayComponent> ent, ref LocalPlayerAttachedEvent args)
    {
        if (!_cfg.GetCVar(DCCVars.NoVisionFilters))
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(Entity<SlasherRegenerateOverlayComponent> ent, ref LocalPlayerDetachedEvent args)
    {
        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnNoVisionFiltersChanged(bool enabled)
    {
        if (enabled)
            _overlayMan.RemoveOverlay(_overlay);
        else if (HasComp<SlasherRegenerateOverlayComponent>(_player.LocalEntity))
            _overlayMan.AddOverlay(_overlay);
    }
}
