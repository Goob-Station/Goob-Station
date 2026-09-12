using Content.Goobstation.Shared.Sandevistan;
using Content.Shared._DV.CCVars;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.Sandevistan;

/// <summary>
/// Handles the sandevistan slowdown overlay.
/// </summary>
public sealed class SandevistanSlowdownVisionSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private SandevistanSlowdownVisionOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SandevistanSlowdownVisionComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<SandevistanSlowdownVisionComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SandevistanSlowdownVisionComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<SandevistanSlowdownVisionComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        Subs.CVar(_cfg, DCCVars.NoVisionFilters, OnNoVisionFiltersChanged);

        _overlay = new();
    }

    private void SetOverlay(bool show)
    {
        if (show && !_cfg.GetCVar(DCCVars.NoVisionFilters))
            _overlayMan.AddOverlay(_overlay);
        else
            _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnInit(Entity<SandevistanSlowdownVisionComponent> ent, ref ComponentInit args)
    {
        if (ent.Owner == _player.LocalEntity)
            SetOverlay(true);
    }

    private void OnShutdown(Entity<SandevistanSlowdownVisionComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Owner == _player.LocalEntity)
            SetOverlay(false);
    }

    private void OnPlayerAttached(Entity<SandevistanSlowdownVisionComponent> ent, ref LocalPlayerAttachedEvent args) =>
        SetOverlay(true);

    private void OnPlayerDetached(Entity<SandevistanSlowdownVisionComponent> ent, ref LocalPlayerDetachedEvent args) =>
        SetOverlay(false);

    private void OnNoVisionFiltersChanged(bool enabled) =>
        SetOverlay(_player.LocalEntity is { } player && HasComp<SandevistanSlowdownVisionComponent>(player));
}
