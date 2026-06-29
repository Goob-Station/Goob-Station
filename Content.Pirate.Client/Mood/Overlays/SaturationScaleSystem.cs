using Content.Shared.CCVar;
using Content.Shared.GameTicking;
using Content.Shared.Overlays;
using Robust.Client.Graphics;
using Robust.Shared.Configuration;
using Robust.Shared.Player;

namespace Content.Pirate.Client.Mood.Overlays;

public sealed class SaturationScaleSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfgMan = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly ISharedPlayerManager _playerMan = default!;

    private SaturationScaleOverlay _overlay = default!;
    private bool _moodEffectsEnabled;

    public override void Initialize()
    {
        base.Initialize();

        _overlay = new SaturationScaleOverlay();
        _moodEffectsEnabled = _cfgMan.GetCVar(CCVars.MoodVisualEffects);
        _cfgMan.OnValueChanged(CCVars.MoodVisualEffects, HandleMoodEffectsUpdated);

        SubscribeLocalEvent<SaturationScaleOverlayComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<SaturationScaleOverlayComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SaturationScaleOverlayComponent, PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<SaturationScaleOverlayComponent, PlayerDetachedEvent>(OnPlayerDetached);
        SubscribeNetworkEvent<RoundRestartCleanupEvent>(RoundRestartCleanup);
    }

    private void HandleMoodEffectsUpdated(bool moodEffectsEnabled)
    {
        if (_overlayMan.HasOverlay<SaturationScaleOverlay>() && !moodEffectsEnabled)
            _overlayMan.RemoveOverlay(_overlay);

        _moodEffectsEnabled = moodEffectsEnabled;
    }

    private void RoundRestartCleanup(RoundRestartCleanupEvent ev)
    {
        if (_moodEffectsEnabled)
            _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnPlayerDetached(EntityUid uid, SaturationScaleOverlayComponent component, PlayerDetachedEvent args)
    {
        if (_moodEffectsEnabled)
            _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnPlayerAttached(EntityUid uid, SaturationScaleOverlayComponent component, PlayerAttachedEvent args)
    {
        if (_moodEffectsEnabled)
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnShutdown(EntityUid uid, SaturationScaleOverlayComponent component, ComponentShutdown args)
    {
        if (uid == _playerMan.LocalEntity && _moodEffectsEnabled)
            _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnInit(EntityUid uid, SaturationScaleOverlayComponent component, ComponentInit args)
    {
        if (uid == _playerMan.LocalEntity && _moodEffectsEnabled)
            _overlayMan.AddOverlay(_overlay);
    }
}
