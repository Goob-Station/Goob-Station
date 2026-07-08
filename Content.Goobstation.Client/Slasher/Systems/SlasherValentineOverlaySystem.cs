using Content.Goobstation.Client.Slasher.Overlays;
using Content.Goobstation.Shared.Slasher.Components;
using Robust.Client.Graphics;
using Robust.Client.Player;

namespace Content.Goobstation.Client.Slasher.Systems;

/// <summary>
/// Drives the Idol's valentine fear grade's intensity from the local victim's fear.
/// </summary>
public sealed class SlasherValentineOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private SlasherValentineOverlay _overlay = default!;

    private const float FadeSpeed = 1.5f;
    private const float BaseIntensity = 0.15f;

    public override void Initialize()
    {
        base.Initialize();

        _overlay = new();
        _overlayMan.AddOverlay(_overlay);
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _overlayMan.RemoveOverlay(_overlay);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var target = TryComp<FearedComponent>(_player.LocalEntity, out var comp)
            ? BaseIntensity + Math.Clamp(comp.Fear, 0f, 1f) * (1f - BaseIntensity)
            : 0f;

        var dt = frameTime * FadeSpeed;
        _overlay.Intensity = MathF.Abs(target - _overlay.Intensity) < dt
            ? target
            : _overlay.Intensity + Math.Sign(target - _overlay.Intensity) * dt;
    }
}
