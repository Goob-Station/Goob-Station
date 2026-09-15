using Content.Goobstation.Shared.Sandevistan;
using Content.Shared._DV.CCVars;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Sandevistan;

/// <summary>
/// This is the grey look the background gets during a sandevistan timestop.
/// Ignores player / item entities.
/// </summary>
public sealed class SandevistanTimestopVisionSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private SandevistanTimestopVisionOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SandevistanTimestopVisionComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<SandevistanTimestopVisionComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SandevistanTimestopVisionComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<SandevistanTimestopVisionComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        Subs.CVar(_cfg, DCCVars.NoVisionFilters, OnNoVisionFiltersChanged);

        _overlay = new();
    }

    private void OnInit(EntityUid uid, SandevistanTimestopVisionComponent component, ComponentInit args)
    {
        if (uid == _player.LocalEntity && !_cfg.GetCVar(DCCVars.NoVisionFilters))
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnShutdown(EntityUid uid, SandevistanTimestopVisionComponent component, ComponentShutdown args)
    {
        if (uid == _player.LocalEntity)
            _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnPlayerAttached(EntityUid uid, SandevistanTimestopVisionComponent component, LocalPlayerAttachedEvent args)
    {
        if (!_cfg.GetCVar(DCCVars.NoVisionFilters))
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(EntityUid uid, SandevistanTimestopVisionComponent component, LocalPlayerDetachedEvent args)
    {
        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnNoVisionFiltersChanged(bool enabled)
    {
        if (enabled)
            _overlayMan.RemoveOverlay(_overlay);
        else
            _overlayMan.AddOverlay(_overlay);
    }
}
