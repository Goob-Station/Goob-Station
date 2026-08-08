using Content.Goobstation.Client.Slasher.Overlays;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Shared._DV.CCVars;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.Slasher.Systems;

/// <summary>
/// Registers the SlasherIncorporealOverlay for the local slasher.
/// </summary>
public sealed class SlasherIncorporealOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private SlasherIncorporealOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherIncorporealComponent, ComponentInit>(OnIncorporealInit);
        SubscribeLocalEvent<SlasherIncorporealComponent, ComponentShutdown>(OnIncorporealShutdown);
        SubscribeLocalEvent<SlasherIncorporealComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<SlasherIncorporealComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        Subs.CVar(_cfg, DCCVars.NoVisionFilters, OnNoVisionFiltersChanged);

        _overlay = new();
    }

    private void OnIncorporealInit(EntityUid uid, SlasherIncorporealComponent component, ComponentInit args)
    {
        if (uid == _player.LocalEntity && !_cfg.GetCVar(DCCVars.NoVisionFilters))
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnIncorporealShutdown(EntityUid uid, SlasherIncorporealComponent component, ComponentShutdown args)
    {
        if (uid == _player.LocalEntity)
            _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnPlayerAttached(EntityUid uid, SlasherIncorporealComponent component, LocalPlayerAttachedEvent args)
    {
        if (!_cfg.GetCVar(DCCVars.NoVisionFilters))
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(EntityUid uid, SlasherIncorporealComponent component, LocalPlayerDetachedEvent args)
    {
        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnNoVisionFiltersChanged(bool enabled)
    {
        if (enabled)
            _overlayMan.RemoveOverlay(_overlay);
        else if (HasComp<SlasherIncorporealComponent>(_player.LocalEntity))
            _overlayMan.AddOverlay(_overlay);
    }
}
