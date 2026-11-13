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
    [Dependency] private IOverlayManager _overlayMan = default!;
    [Dependency] private IConfigurationManager _cfg = default!;

    private readonly CinematicLetterboxOverlay _overlay = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CinematicLetterboxComponent, ComponentInit>(OnLetterboxInit);
        SubscribeLocalEvent<CinematicLetterboxComponent, ComponentShutdown>(OnLetterboxShutdown);
        SubscribeLocalEvent<CinematicLetterboxComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<CinematicLetterboxComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);
        SubscribeLocalEvent<CinematicLetterboxComponent, CinematicUpdatedEvent>(OnCinematicUpdated);

        Subs.CVar(_cfg, DCCVars.NoVisionFilters, OnNoVisionFiltersChanged);
    }

    private void OnLetterboxInit(Entity<CinematicLetterboxComponent> ent, ref ComponentInit args)
    {
        if (!_cfg.GetCVar(DCCVars.NoVisionFilters))
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnLetterboxShutdown(Entity<CinematicLetterboxComponent> ent, ref ComponentShutdown args)
    {
        if (EntityManager.Count<CinematicLetterboxComponent>() <= 1)
            _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnPlayerAttached(Entity<CinematicLetterboxComponent> ent, ref LocalPlayerAttachedEvent args)
    {
        if (!_cfg.GetCVar(DCCVars.NoVisionFilters))
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(Entity<CinematicLetterboxComponent> ent, ref LocalPlayerDetachedEvent args)
        => _overlayMan.RemoveOverlay(_overlay);

    private void OnCinematicUpdated(Entity<CinematicLetterboxComponent> ent, ref CinematicUpdatedEvent args)
        => ent.Comp.Strength = args.Strength;

    private void OnNoVisionFiltersChanged(bool enabled)
    {
        if (enabled)
            _overlayMan.RemoveOverlay(_overlay);
        else
            _overlayMan.AddOverlay(_overlay);
    }
}
