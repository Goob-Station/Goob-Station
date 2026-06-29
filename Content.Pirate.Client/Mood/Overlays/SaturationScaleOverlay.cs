using System.Numerics;
using Content.Shared.Overlays;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Pirate.Client.Mood.Overlays;

public sealed class SaturationScaleOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public override bool RequestScreenTexture => true;
    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    private readonly ShaderInstance _shader;
    private float _currentSaturation = 1f;

    public SaturationScaleOverlay()
    {
        IoCManager.InjectDependencies(this);
        _shader = _prototypeManager.Index<ShaderPrototype>("SaturationScale").Instance().Duplicate();
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (_playerManager.LocalEntity is not { Valid: true } player
            || !_entityManager.HasComponent<SaturationScaleOverlayComponent>(player))
            return false;

        return base.BeforeDraw(in args);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture is null
            || _playerManager.LocalEntity is not { Valid: true } player
            || !_entityManager.HasComponent<SaturationScaleOverlayComponent>(player))
            return;

        _shader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        _shader.SetParameter("saturation", _currentSaturation);

        var handle = args.WorldHandle;
        handle.SetTransform(Matrix3x2.Identity);
        handle.UseShader(_shader);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        if (ScreenTexture is null
            || _playerManager.LocalEntity is not { Valid: true } player
            || !_entityManager.TryGetComponent(player, out SaturationScaleOverlayComponent? saturationComp)
            || _currentSaturation == saturationComp.SaturationScale)
            return;

        var target = saturationComp.SaturationScale;
        var delta = args.DeltaSeconds * saturationComp.FadeInMultiplier;
        _currentSaturation = MathHelper.CloseToPercent(_currentSaturation, target, 0.001f)
            ? target
            : Math.Clamp(_currentSaturation + (target > _currentSaturation ? delta : -delta),
                Math.Min(_currentSaturation, target),
                Math.Max(_currentSaturation, target));
        _shader.SetParameter("saturation", _currentSaturation);
    }
}
