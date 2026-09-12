using Content.Goobstation.Shared.Cinematic;
using Content.Shared._DV.CCVars;
using Robust.Client.Graphics;
using Robust.Shared.Configuration;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.Cinematic;

/// <summary>
/// Shows cinematic black bars while any letterbox is running.
/// </summary>
public sealed partial class CinematicLetterboxOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private CinematicLetterboxOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CinematicLetterboxComponent, ComponentInit>(OnLetterboxInit);
        SubscribeLocalEvent<CinematicLetterboxComponent, ComponentShutdown>(OnLetterboxShutdown);
        SubscribeLocalEvent<CinematicLetterboxComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<CinematicLetterboxComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);
        SubscribeLocalEvent<CinematicLetterboxComponent, CinematicUpdatedEvent>(OnCinematicUpdated);

        Subs.CVar(_cfg, DCCVars.NoVisionFilters, OnNoVisionFiltersChanged);

        _overlay = new();
    }

    private void OnLetterboxInit(EntityUid uid, CinematicLetterboxComponent component, ComponentInit args)
    {
        if (!_cfg.GetCVar(DCCVars.NoVisionFilters))
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnLetterboxShutdown(EntityUid uid, CinematicLetterboxComponent component, ComponentShutdown args)
    {
        if (EntityManager.Count<CinematicLetterboxComponent>() <= 1)
            _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnPlayerAttached(EntityUid uid, CinematicLetterboxComponent component, LocalPlayerAttachedEvent args)
    {
        if (!_cfg.GetCVar(DCCVars.NoVisionFilters))
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(EntityUid uid, CinematicLetterboxComponent component, LocalPlayerDetachedEvent args) =>
        _overlayMan.RemoveOverlay(_overlay);

    private void OnCinematicUpdated(EntityUid uid, CinematicLetterboxComponent component, ref CinematicUpdatedEvent args) =>
        component.Strength = args.Strength;

    private void OnNoVisionFiltersChanged(bool enabled)
    {
        if (enabled)
            _overlayMan.RemoveOverlay(_overlay);
        else
            _overlayMan.AddOverlay(_overlay);
    }
}
