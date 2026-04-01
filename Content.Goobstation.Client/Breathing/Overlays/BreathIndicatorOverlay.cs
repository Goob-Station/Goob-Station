using System.Numerics;
using Content.Goobstation.Shared.Breathing;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Shared.Enums;

namespace Content.Goobstation.Client.Breathing.Overlays;

public sealed class BreathIndicatorOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IEntitySystemManager _sysMan = default!;

    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    private const float BarWidth = 150f;
    private const float BarHeight = 8f;
    private const float BarYOffset = 120f;

    private float _progress;

    public BreathIndicatorOverlay()
    {
        IoCManager.InjectDependencies(this);
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        var playerEntity = _playerManager.LocalEntity;
        if (playerEntity == null)
            return false;

        if (!_entityManager.TryGetComponent<ManualBreathingComponent>(playerEntity, out var comp))
            return false;

        var breathSys = _sysMan.GetEntitySystem<ManualBreathingSystem>();
        _progress = breathSys.GetBreathProgress(comp);

        return _progress > 0.01f;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.ScreenHandle;
        var uiScale = (args.ViewportControl as Control)?.UIScale ?? 1f;

        var screenSize = (args.ViewportControl as Control)?.PixelSize ?? new Vector2i(1920, 1080);
        var barWidth = BarWidth * uiScale;
        var barHeight = BarHeight * uiScale;
        var yOffset = BarYOffset * uiScale;

        var barPos = new Vector2(
            (screenSize.X - barWidth) / 2f,
            screenSize.Y - yOffset - barHeight
        );

        handle.DrawRect(new UIBox2(barPos, barPos + new Vector2(barWidth, barHeight)), Color.Black.WithAlpha(0.5f));

        Color fillColor;
        if (_progress <= 0.6f)
            fillColor = Color.InterpolateBetween(Color.Green, Color.Yellow, _progress / 0.6f);
        else if (_progress <= 1.0f)
            fillColor = Color.InterpolateBetween(Color.Yellow, Color.Red, (_progress - 0.6f) / 0.4f);
        else
            fillColor = Color.Red;

        var fillWidth = barWidth * _progress;
        handle.DrawRect(new UIBox2(barPos, barPos + new Vector2(fillWidth, barHeight)), fillColor);

        handle.DrawRect(new UIBox2(barPos, barPos + new Vector2(barWidth, barHeight)), Color.White.WithAlpha(0.3f), filled: false);
    }
}
