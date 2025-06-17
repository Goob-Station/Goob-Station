using System.Numerics;
using Content.Client.Resources;
using Content.Goobstation.Common.Style;
using Content.Goobstation.Shared.Style;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.Style;

public sealed class StyleHudOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IResourceCache _resourceCache = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    private readonly IPlayerManager _player;

    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    public StyleHudOverlay(IPlayerManager player)
    {
        _player = player;
        IoCManager.InjectDependencies(this);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.ScreenHandle;
        var playerEntity = _player.LocalEntity;

        if (playerEntity == null
            || !_entityManager.TryGetComponent(playerEntity.Value, out StyleCounterComponent? style))
            return;

        var rankProto = _prototypeManager.Index<StyleRankPrototype>(style.Rank.ToString());
        var rankText = rankProto.DisplayText;
        var rankColor = rankProto.Color;

        var screenSize = args.ViewportBounds.Size;

        // calculate box dimensions and position (relative to screen size)
        const float boxWidthPercentage = 0.2f;
        const float boxHeightPercentage = 0.3f;
        const float boxLeftPercentage = 0.065f;
        const float boxTopPercentage = 0.1f;

        var boxWidth = screenSize.X * boxWidthPercentage;
        var boxHeight = screenSize.Y * boxHeightPercentage;
        var boxLeft = screenSize.X * boxLeftPercentage;
        var boxTop = screenSize.Y * boxTopPercentage;

        var box = new UIBox2(
            new Vector2(boxLeft, boxTop),
            new Vector2(boxLeft + boxWidth, boxTop + boxHeight));

        // draw main box
        handle.DrawRect(box, new Color(0, 0, 0, 180));
        DrawBorder(handle, box, rankColor);

        var fontSize = Math.Max(12, screenSize.Y / 60);
        var font = _resourceCache.GetFont("/Fonts/_Goida/VCR_OSD_MONO_1.001.ttf", fontSize);

        // rank text
        var rankPos = new Vector2(boxLeft + boxWidth * 0.05f, boxTop + boxHeight * 0.1f);
        handle.DrawString(font, rankPos, rankText, rankColor);

        //multiplier
        var multiplierPos = new Vector2(boxLeft + boxWidth * 0.05f, boxTop + boxHeight * 0.2f);
        handle.DrawString(font, multiplierPos, $"Multiplier: x{style.CurrentMultiplier:F1}", Color.LightGray);

        // draw recent events if any
        if (style.RecentEvents.Count > 0)
        {
            DrawRecentEvents(handle, font, box, fontSize, style.RecentEvents);
        }
    }

    private void DrawBorder(DrawingHandleScreen handle, UIBox2 box, Color color)
    {
        const float borderThickness = 1f;

        // tpp border
        handle.DrawRect(
            new UIBox2(
                new Vector2(box.Left - borderThickness, box.Top - borderThickness),
                new Vector2(box.Right + borderThickness, box.Top)),
            color);

        // bottom border
        handle.DrawRect(
            new UIBox2(
                new Vector2(box.Left - borderThickness, box.Bottom),
                new Vector2(box.Right + borderThickness, box.Bottom + borderThickness)),
            color);

        // left border
        handle.DrawRect(
            new UIBox2(
                new Vector2(box.Left - borderThickness, box.Top),
                new Vector2(box.Left, box.Bottom)),
            color);

        // right border
        handle.DrawRect(
            new UIBox2(
                new Vector2(box.Right, box.Top),
                new Vector2(box.Right + borderThickness, box.Bottom)),
            color);
    }

    private void DrawRecentEvents(
        DrawingHandleScreen handle,
        Font font,
        UIBox2 box,
        float fontSize,
        IList<string> recentEvents)
    {
        var eventsPos = new Vector2(box.Left + box.Width * 0.05f, box.Top + box.Height * 0.3f);
        var maxEvents = (int) ((box.Height * 0.7f) / (fontSize * 1.5f));

        for (int i = 0; i < Math.Min(maxEvents, recentEvents.Count); i++)
        {
            var index = recentEvents.Count - 1 - i;
            var (message, color) = ProcessEventMessage(recentEvents[index]);

            handle.DrawString(
                font,
                eventsPos + new Vector2(0, i * fontSize * 1.5f),
                message,
                color);
        }
    }

    private (string message, Color color) ProcessEventMessage(string rawMessage)
    {
        const string colorTagStart = "[color=";
        const string colorTagEnd = "]";
        const string colorCloseTag = "[/color]";

        // default values in case of something goes wrong
        var message = rawMessage;
        var color = Color.White;

        if (message.Contains(colorTagStart) && message.Contains(colorTagEnd))
        {
            try
            {
                var colorStart = message.IndexOf(colorTagStart, StringComparison.Ordinal) + colorTagStart.Length;
                var colorEnd = message.IndexOf(colorTagEnd, colorStart, StringComparison.Ordinal);
                var colorHex = message[colorStart..colorEnd];

                var parsedColor = Color.TryFromHex(colorHex);
                if (parsedColor != null)
                {
                    color = parsedColor.Value;
                    var textStart = message.IndexOf(colorTagEnd, colorEnd, StringComparison.Ordinal) +
                                    colorTagEnd.Length;
                    var textEnd = message.IndexOf(colorCloseTag, textStart, StringComparison.Ordinal);
                    message = message.Substring(textStart, textEnd - textStart);
                }
            }
            catch
            {
                // thank you rider
            }
        }

        return (message, color);
    }
}
