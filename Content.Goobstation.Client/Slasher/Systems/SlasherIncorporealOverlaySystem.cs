using Content.Goobstation.Client.Slasher.Overlays;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Shared._DV.CCVars;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.Slasher.Systems;

public sealed class SlasherIncorporealOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private readonly SlasherIncorporealOverlay _overlay = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherIncorporealOverlayComponent, ComponentStartup>(OnOverlayStartup);
        SubscribeLocalEvent<SlasherIncorporealOverlayComponent, ComponentShutdown>(OnOverlayShutdown);
        SubscribeLocalEvent<SlasherIncorporealOverlayComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<SlasherIncorporealOverlayComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        Subs.CVar(_cfg, DCCVars.NoVisionFilters, OnNoVisionFiltersChanged);
    }

    private void OnOverlayStartup(Entity<SlasherIncorporealOverlayComponent> ent, ref ComponentStartup args)
    {
        if (ent.Owner != _player.LocalEntity || _cfg.GetCVar(DCCVars.NoVisionFilters))
            return;

        _overlay.Intensity = 0f;
        _overlayMan.AddOverlay(_overlay);
    }

    private void OnOverlayShutdown(Entity<SlasherIncorporealOverlayComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Owner == _player.LocalEntity)
            _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnPlayerAttached(Entity<SlasherIncorporealOverlayComponent> ent, ref LocalPlayerAttachedEvent args)
    {
        if (_cfg.GetCVar(DCCVars.NoVisionFilters))
            return;

        _overlay.Intensity = 0f;
        _overlayMan.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(Entity<SlasherIncorporealOverlayComponent> ent, ref LocalPlayerDetachedEvent args)
    {
        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnNoVisionFiltersChanged(bool enabled)
    {
        if (enabled)
            _overlayMan.RemoveOverlay(_overlay);
        else if (HasComp<SlasherIncorporealOverlayComponent>(_player.LocalEntity))
            _overlayMan.AddOverlay(_overlay);
    }
}
