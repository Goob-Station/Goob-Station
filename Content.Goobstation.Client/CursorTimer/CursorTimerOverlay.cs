using Content.Client.Resources;
using Content.Goobstation.Shared.CursorTimer;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.CursorTimer;

public sealed partial class CursorTimerOverlay : Overlay
{
    [Dependency] private IEntityManager _entMan = default!;
    [Dependency] private IInputManager _input = default!;
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IResourceCache _cache = default!;

    private readonly Dictionary<(string Path, int Size), Font> _fonts = new();
    private readonly EntityQuery<CursorTimerComponent> _timerQuery;

    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    public CursorTimerOverlay()
    {
        IoCManager.InjectDependencies(this);

        _timerQuery = _entMan.GetEntityQuery<CursorTimerComponent>();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (_player.LocalEntity is not { } player
            || !_timerQuery.TryGetComponent(player, out var timer))
            return;

        var mousePos = _input.MouseScreenPosition;
        if (mousePos.Window == WindowId.Invalid)
            return;

        var remaining = MathF.Max(0f, (float) (timer.EndTime - _timing.CurTime).TotalSeconds);

        var uiScale = (args.ViewportControl as Control)?.UIScale ?? 1f;
        var text = remaining.ToString($"F{timer.Decimals}");
        var pos = mousePos.Position + timer.Offset * uiScale;

        var color = timer.Color;
        if (timer.WarningSeconds > 0f)
            color = Color.InterpolateBetween(timer.WarningColor, timer.Color, Math.Clamp(remaining / timer.WarningSeconds, 0f, 1f));

        args.ScreenHandle.DrawString(GetFont(timer), pos, text, uiScale, color, TextOutline.Default);
    }

    private Font GetFont(CursorTimerComponent timer)
    {
        var key = (timer.FontPath, timer.FontSize);
        if (!_fonts.TryGetValue(key, out var font))
            _fonts[key] = font = _cache.GetFont(timer.FontPath, timer.FontSize);

        return font;
    }
}
