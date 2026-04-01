// SPDX-FileCopyrightText: 2026 Site-14 Contributors
//
// SPDX-License-Identifier: MPL-2.0
//
// Additional Use Restrictions apply:
// See /LICENSES/SITE14-ADDENDUM.md

using System.Numerics;
using Content.Goobstation.Shared.Blinking;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Shared.Enums;
using Robust.Shared.Maths;

namespace Content.Goobstation.Client.Blinking.Overlays;

public sealed class BlinkIndicatorOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IEntitySystemManager _sysMan = default!;

    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    private const float BarWidth = 150f;
    private const float BarHeight = 8f;
    private const float BarYOffset = 100f;

    private float _urgency;

    public BlinkIndicatorOverlay()
    {
        IoCManager.InjectDependencies(this);
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        var playerEntity = _playerManager.LocalEntity;
        if (playerEntity == null)
            return false;

        if (!_entityManager.TryGetComponent<BlinkingComponent>(playerEntity, out var blinkComp))
            return false;

        if (!blinkComp.AutoBlink)
            return false;

        var blinkingSys = _sysMan.GetEntitySystem<BlinkingSystem>();
        _urgency = blinkingSys.GetBlinkUrgency(blinkComp);

        return _urgency > 0.01f;
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

        var bgColor = Color.Black.WithAlpha(0.5f);
        handle.DrawRect(new UIBox2(barPos, barPos + new Vector2(barWidth, barHeight)), bgColor);

        var fillColor = Color.InterpolateBetween(Color.Yellow, Color.Red, _urgency);
        var fillWidth = barWidth * _urgency;
        handle.DrawRect(new UIBox2(barPos, barPos + new Vector2(fillWidth, barHeight)), fillColor);

        var borderColor = Color.White.WithAlpha(0.3f);
        handle.DrawRect(new UIBox2(barPos, barPos + new Vector2(barWidth, barHeight)), borderColor, filled: false);
    }
}
