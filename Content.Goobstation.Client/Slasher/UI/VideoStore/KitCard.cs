using System.Numerics;
using Content.Goobstation.Shared.Slasher.UI;
using Content.Shared.Guidebook;
using Robust.Client.Audio;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Shared.Graphics;
using Robust.Shared.Input;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Slasher.UI.VideoStore;

public sealed class KitCard : Control
{
    [Dependency] private readonly IClyde _clyde = default!;
    [Dependency] private readonly IEntitySystemManager _sysMan = default!;
    [Dependency] private readonly IResourceCache _cache = default!;

    public event Action<string>? OnRent;
    public event Action<ProtoId<GuideEntryPrototype>>? OnGuide;
    public event Action? OnSequels;
    public event Action? OnPrequel;

    private const float StringLength = 70f;
    private const float BodyX = 10f;
    private const float BodyY = StringLength;
    private const float BodyWidth = 310f;
    private const float BodyHeight = 560f;
    private const float Inset = 16f;
    private const float Inner = BodyWidth - Inset * 2f;
    private const float TvTop = 250f;
    private const float TvHeight = 110f;
    private const float ButtonHeight = 30f;
    private const float ButtonGap = 6f;
    private const float SwingDegrees = 2f;
    private const float SwingPeriod = 6f;
    private const float ConfirmTimeout = 3f;
    private const float LeadPad = 5f;

    private const string PrequelChevron = "<";
    private const string SequelsChevron = ">";

    private readonly AudioSystem _audio;
    private readonly SpriteSystem _sprites;

    private readonly Texture _card;
    private readonly Texture _tv;
    private readonly Texture _static;
    private readonly StyleBoxTexture _ghost;
    private readonly StyleBoxTexture _dark;
    private readonly StyleBoxTexture _rent;
    private readonly StyleBoxTexture _sure;

    private readonly Font _titleFont;
    private readonly Font _subFont;
    private readonly Font _leadFont;
    private readonly Font _bodyFont;
    private readonly Font _osdFont;
    private readonly Font _tvNameFont;
    private readonly Font _buttonFont;
    private readonly Font _rentFont;

    private SlasherKitInfo? _kit;
    private Texture? _icon;
    private string _title = string.Empty;
    private string _sub = string.Empty;
    private string? _note;
    private string? _lead;
    private string _variantDescription = string.Empty;
    private int _sequels;
    private int _depth;

    private float _wrappedAt = -1f;
    private List<string> _subLines = new();
    private List<string>? _leadLines;
    private List<string> _bodyWide = new();
    private List<string>? _bodyNarrow;

    private int _scroll;
    private int _scrollMax;

    private readonly List<ButtonEntry> _stack = new();
    private readonly List<(CardButton Button, UIBox2 Rect)> _buttons = new();
    private CardButton? _hovered;
    private float _time;

    private IRenderTexture? _target;
    private EntityUid? _stream;
    private bool _confirming;
    private float _confirmTimer;

    public KitCard()
    {
        IoCManager.InjectDependencies(this);
        _audio = _sysMan.GetEntitySystem<AudioSystem>();
        _sprites = _sysMan.GetEntitySystem<SpriteSystem>();

        _card = StoreTextures.Get(_cache, "card");
        _tv = StoreTextures.Get(_cache, "tv");
        _static = StoreTextures.Get(_cache, "tv_static");
        _ghost = StoreTextures.NinePatch(_cache, "btn_ghost");
        _dark = StoreTextures.NinePatch(_cache, "btn_dark");
        _rent = StoreTextures.NinePatch(_cache, "btn_rent");
        _sure = StoreTextures.NinePatch(_cache, "btn_sure");

        _titleFont = StoreFonts.Sign(_cache, 34);
        _subFont = StoreFonts.Term(_cache, 14);
        _leadFont = StoreFonts.Stencil(_cache, 12);
        _bodyFont = StoreFonts.Term(_cache, 19);
        _osdFont = StoreFonts.Term(_cache, 13);
        _tvNameFont = StoreFonts.Drip(_cache, 13);
        _buttonFont = StoreFonts.Sign(_cache, 19);
        _rentFont = StoreFonts.Sign(_cache, 23);

        MouseFilter = MouseFilterMode.Stop;
        SetSize = new Vector2(330f, 650f);
    }

    public void Show(SlasherKitInfo kit, string? parentName, int depth, int sequels)
    {
        StopTrailer();
        _kit = kit;
        _sequels = sequels;
        _depth = depth;
        _icon = _sprites.Frame0(kit.Sprite);
        _title = Loc.GetString(kit.Name).ToUpperInvariant();
        _confirming = false;

        var parent = (parentName ?? kit.RequiredAscension ?? string.Empty).ToUpperInvariant();
        if (depth > 0)
            _sub = Loc.GetString("slasher-kit-store-sequel-to", ("part", StoreFonts.Roman(depth + 1)), ("kit", parent));
        else if (sequels > 0)
            _sub = Loc.GetString("slasher-kit-store-original-sequels", ("count", sequels));
        else
            _sub = Loc.GetString("slasher-kit-store-original");

        _note = kit.Guide != null ? Loc.GetString("slasher-kit-store-plays-differently") : null;
        _lead = kit.Unlocked ? null : Loc.GetString("slasher-kit-store-need", ("kit", parent));
        _variantDescription = kit.Description is { } description ? Loc.GetString(description) : string.Empty;
        _scroll = 0;
        _wrappedAt = -1f;
        RebuildButtons();
    }

    protected override void MouseWheel(GUIMouseWheelEventArgs args)
    {
        base.MouseWheel(args);
        if (_scrollMax <= 0)
            return;

        _scroll = Math.Clamp(_scroll - Math.Sign(args.Delta.Y), 0, _scrollMax);
        args.Handle();
    }

    private Matrix3x2 Swing(float s)
    {
        var angle = MathHelper.DegreesToRadians(SwingDegrees) * MathF.Sin(_time * MathF.Tau / SwingPeriod);
        var pivot = new Vector2(Width * 0.5f, 0f) * s;
        return Matrix3Helpers.CreateTranslation(-pivot) * Matrix3Helpers.CreateTransform(pivot, new Angle(angle));
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        _time += args.DeltaSeconds;

        if (!_confirming)
            return;

        _confirmTimer -= args.DeltaSeconds;
        if (_confirmTimer <= 0f)
            SetConfirming(false);
    }

    private CardButton? ButtonAt(Vector2 pixel)
    {
        if (!Matrix3x2.Invert(Swing(UIScale), out var inverse))
            return null;

        var local = Vector2.Transform(pixel, inverse);
        foreach (var (button, rect) in _buttons)
            if (rect.Contains(local))
                return button;

        return null;
    }

    protected override void MouseMove(GUIMouseMoveEventArgs args)
    {
        base.MouseMove(args);

        _hovered = ButtonAt(args.RelativePixelPosition);
    }

    protected override void MouseExited()
    {
        base.MouseExited();

        _hovered = null;
    }

    protected override void KeyBindDown(GUIBoundKeyEventArgs args)
    {
        base.KeyBindDown(args);

        if (args.Function != EngineKeyFunctions.UIClick || _kit == null)
            return;

        var hit = ButtonAt(args.RelativePixelPosition);
        if (hit == null)
            return;

        args.Handle();
        switch (hit)
        {
            case CardButton.Trailer:
                ToggleTrailer();
                break;
            case CardButton.Guide:
                if (_kit.Guide is { } guide)
                    OnGuide?.Invoke(guide);
                break;
            case CardButton.Sequels:
                OnSequels?.Invoke();
                break;
            case CardButton.Prequel:
                OnPrequel?.Invoke();
                break;
            case CardButton.Rent:
                PressRent();
                break;
        }
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        if (_kit == null)
            return;

        var s = UIScale;
        var size = PixelSize;
        if (_target == null || _target.Size != size)
        {
            _target?.Dispose();
            _target = _clyde.CreateRenderTarget(size,
                new RenderTargetFormatParameters(RenderTargetColorFormat.Rgba8Srgb, true),
                new TextureSampleParameters { Filter = true },
                nameof(KitCard));
        }

        var old = handle.GetTransform();
        handle.RenderInRenderTarget(_target, () =>
        {
            handle.SetTransform(Matrix3x2.Identity);
            DrawContents(handle, s);
        }, Color.Transparent);

        handle.SetTransform(Swing(s) * old);
        handle.DrawTextureRect(_target.Texture, UIBox2.FromDimensions(Vector2.Zero, new Vector2(size.X, size.Y)));
        handle.SetTransform(old);
    }

    private void DrawContents(DrawingHandleScreen handle, float s)
    {
        if (_wrappedAt != s)
            Rewrap(s);

        var hook = new Vector2(Width * 0.5f, 0f) * s;
        handle.DrawLine(hook, hook + new Vector2(0f, StringLength * s), StorePalette.Steel);
        handle.DrawTextureRect(_card, UIBox2.FromDimensions(
            new Vector2(BodyX - 2f, BodyY - 2f) * s,
            new Vector2(_card.Width, _card.Height) * s));

        var x = (BodyX + Inset) * s;
        var y = (BodyY + 14f) * s;

        var titleScale = FitScale(_titleFont, _title, Inner * s, s);
        handle.DrawString(_titleFont, new Vector2(x, y), _title, titleScale, StorePalette.Ink);
        y += _titleFont.GetLineHeight(s) + 2f * s;

        foreach (var line in _subLines)
        {
            handle.DrawString(_subFont, new Vector2(x, y), line, s, StorePalette.Wine);
            y += _subFont.GetLineHeight(s);
        }

        y += 6f * s;
        handle.DrawRect(UIBox2.FromDimensions(new Vector2(x, y), new Vector2(Inner * s, 2f * s)), StorePalette.Ink);
        y += 10f * s;

        if (_leadLines != null)
        {
            var pad = LeadPad * s;
            var height = _leadLines.Count * _leadFont.GetLineHeight(s) + pad * 2f;
            handle.DrawRect(UIBox2.FromDimensions(new Vector2(x, y), new Vector2(Inner * s, height)), StorePalette.Red);
            var ly = y + pad;
            foreach (var line in _leadLines)
            {
                handle.DrawString(_leadFont, new Vector2(x + pad, ly), line, s, Color.White);
                ly += _leadFont.GetLineHeight(s);
            }

            y += height + 6f * s;
        }

        var limit = (BodyY + TvTop - 6f) * s;
        var lineHeight = _bodyFont.GetLineHeight(s);
        var visible = Math.Max(0, (int) ((limit - y) / lineHeight));
        var barWidth = 6f * s;
        var body = _bodyWide;
        if (body.Count > visible)
            body = _bodyNarrow ??= StoreFonts.Wrap(_bodyFont, _variantDescription, Inner * s - barWidth - 6f * s, s);
        _scrollMax = visible > 0 ? Math.Max(0, body.Count - visible) : 0;
        _scroll = Math.Clamp(_scroll, 0, _scrollMax);

        var top = y;
        for (var i = _scroll; i < Math.Min(body.Count, _scroll + visible); i++)
        {
            handle.DrawString(_bodyFont, new Vector2(x, y), body[i], s, StorePalette.Ink);
            y += lineHeight;
        }

        if (_scrollMax > 0)
        {
            var trackHeight = visible * lineHeight;
            var trackTop = new Vector2(x + Inner * s - barWidth, top);
            var track = UIBox2.FromDimensions(trackTop, new Vector2(barWidth, trackHeight));
            handle.DrawRect(track, StorePalette.Ink.WithAlpha(0.18f));
            var thumbHeight = MathF.Max(12f * s, trackHeight * visible / body.Count);
            var thumbTop = top + (trackHeight - thumbHeight) * _scroll / _scrollMax;
            var thumb = UIBox2.FromDimensions(new Vector2(track.Left, thumbTop), new Vector2(barWidth, thumbHeight));
            handle.DrawRect(thumb, StorePalette.Wine);
        }

        DrawTv(handle, new Vector2(x, (BodyY + TvTop) * s), s);
        DrawButtons(handle, x, s);
    }

    private void Rewrap(float s)
    {
        _wrappedAt = s;
        _subLines = StoreFonts.Wrap(_subFont, _sub, Inner * s, s);
        if (_note != null)
            _subLines.AddRange(StoreFonts.Wrap(_subFont, _note, Inner * s, s));
        _leadLines = _lead == null ? null : StoreFonts.Wrap(_leadFont, _lead, (Inner - LeadPad * 2f) * s, s);
        _bodyWide = StoreFonts.Wrap(_bodyFont, _variantDescription, Inner * s, s);
        _bodyNarrow = null;
    }

    private static float FitScale(Font font, string text, float max, float s)
    {
        var width = StoreFonts.MeasureWidth(font, text, s);
        return width > max ? s * max / width : s;
    }

    private void DrawTv(DrawingHandleScreen handle, Vector2 at, float s)
    {
        var rect = UIBox2.FromDimensions(at, new Vector2(Inner, TvHeight) * s);
        handle.DrawTextureRect(_tv, rect);

        var screen = UIBox2.FromDimensions(at + new Vector2(9f, 9f) * s, new Vector2(Inner - 18f, TvHeight - 18f) * s);
        var unlocked = _kit!.Unlocked;

        if (_icon != null)
        {
            var size = 80f * s;
            var center = new Vector2(screen.Left + screen.Width * 0.5f, screen.Top + screen.Height * 0.5f - 5f * s);
            var wobble = new Vector2(0f, MathF.Sin(_time * 1.3f) * 2f * s);
            var box = UIBox2.FromDimensions(center - new Vector2(size * 0.5f) + wobble, new Vector2(size));
            var alpha = unlocked ? 1f : 0.12f;
            var fringe = new Vector2(3f * s, 0f);
            handle.DrawTextureRect(_icon, box.Translated(-fringe), StorePalette.Pink.WithAlpha(0.6f * alpha));
            handle.DrawTextureRect(_icon, box.Translated(fringe), StorePalette.Cyan.WithAlpha(0.6f * alpha));
            handle.DrawTextureRect(_icon, box, Color.White.WithAlpha(alpha));
        }

        if (!unlocked)
            handle.DrawTextureRect(_static, screen, Color.White.WithAlpha(0.7f + 0.15f * MathF.Sin(_time * 23f)));

        var nowPlaying = Loc.GetString("slasher-kit-store-now-playing");
        handle.DrawString(_osdFont, screen.TopLeft + new Vector2(6f, 4f) * s, nowPlaying, s, StorePalette.Green);
        var tag = Loc.GetString("slasher-kit-store-trailer-tag");
        var tagWidth = StoreFonts.MeasureWidth(_osdFont, tag, s);
        var tagPos = new Vector2(screen.Right - 6f * s - tagWidth, screen.Top + 4f * s);
        handle.DrawString(_osdFont, tagPos, tag, s, StorePalette.Red);

        var name = unlocked ? _title : Loc.GetString("slasher-kit-store-tape-not-found");
        var nameScale = FitScale(_tvNameFont, name, screen.Width - 12f * s, s);
        var width = StoreFonts.MeasureWidth(_tvNameFont, name, nameScale);
        var pos = new Vector2(
            screen.Left + (screen.Width - width) * 0.5f,
            screen.Bottom - _tvNameFont.GetLineHeight(nameScale) - 4f * s);
        handle.DrawString(_tvNameFont, pos + new Vector2(2f, 2f) * s, name, nameScale, Color.Black);
        handle.DrawString(_tvNameFont, pos, name, nameScale, StorePalette.Red);
    }

    private void RebuildButtons()
    {
        _stack.Clear();
        if (_kit is not { } kit)
            return;

        if (kit.Guide != null)
            Add(CardButton.Guide, "slasher-kit-store-guide", _ghost, _buttonFont, StorePalette.Ink, true);
        if (_depth > 0)
            Add(CardButton.Prequel, "slasher-kit-store-prequel", _dark, _buttonFont, StorePalette.Cyan, true);

        var sequels = _sequels > 0 ? "slasher-kit-store-sequels" : "slasher-kit-store-no-sequels";
        Add(CardButton.Sequels, sequels, _dark, _buttonFont, StorePalette.Yellow, _sequels > 0, ("count", _sequels));

        var trailer = _stream != null ? "slasher-kit-store-trailer-stop" : "slasher-kit-store-trailer";
        Add(CardButton.Trailer, trailer, _ghost, _buttonFont, StorePalette.Ink, kit.ThemeSong != null);

        if (!kit.Unlocked)
            Add(CardButton.Rent, "slasher-kit-store-unavailable", _rent, _rentFont, Color.White, false);
        else if (_confirming)
            Add(CardButton.Rent, "slasher-kit-store-rent-confirm", _sure, _rentFont, StorePalette.Deep, true);
        else
            Add(CardButton.Rent, "slasher-kit-store-rent", _rent, _rentFont, Color.White, true);

        void Add(
            CardButton button,
            string key,
            StyleBoxTexture style,
            Font font,
            Color color,
            bool enabled,
            params (string, object)[] args)
        {
            _stack.Add(new ButtonEntry(button, Loc.GetString(key, args), style, font, color, enabled));
        }
    }

    private void DrawButtons(DrawingHandleScreen handle, float x, float s)
    {
        _buttons.Clear();

        var y = (BodyY + BodyHeight - Inset) * s;
        for (var i = _stack.Count - 1; i >= 0; i--)
        {
            var (button, text, style, font, color, enabled) = _stack[i];
            y -= ButtonHeight * s;
            var rect = UIBox2.FromDimensions(new Vector2(x, y), new Vector2(Inner, ButtonHeight) * s);

            style.Draw(handle, rect, s);
            if (!enabled)
                handle.DrawRect(rect, StorePalette.Cream.WithAlpha(0.55f));
            else if (_hovered == button)
                handle.DrawRect(rect, Color.White.WithAlpha(0.18f));

            var ink = enabled ? color : color.WithAlpha(0.6f);
            var textTop = rect.Top + (rect.Height - font.GetLineHeight(s)) * 0.5f;
            var width = StoreFonts.MeasureWidth(font, text, s);
            handle.DrawString(font, new Vector2(rect.Left + (rect.Width - width) * 0.5f, textTop), text, s, ink);

            if (button == CardButton.Prequel)
                handle.DrawString(font, new Vector2(rect.Left + 10f * s, textTop), PrequelChevron, s, ink);
            else if (button == CardButton.Sequels && enabled)
            {
                var chevronWidth = StoreFonts.MeasureWidth(font, SequelsChevron, s);
                var chevronPos = new Vector2(rect.Right - 10f * s - chevronWidth, textTop);
                handle.DrawString(font, chevronPos, SequelsChevron, s, ink);
            }

            if (enabled)
                _buttons.Add((button, rect));
            y -= ButtonGap * s;
        }
    }

    private void PressRent()
    {
        if (_kit == null || !_kit.Unlocked)
            return;

        if (!_confirming)
        {
            SetConfirming(true);
            return;
        }

        OnRent?.Invoke(_kit.Id);
    }

    private void SetConfirming(bool confirming)
    {
        _confirming = confirming;
        _confirmTimer = ConfirmTimeout;
        RebuildButtons();
    }

    private void ToggleTrailer()
    {
        if (_stream != null)
        {
            StopTrailer();
            return;
        }

        if (_kit?.ThemeSong is not { } song)
            return;

        _stream = _audio.PlayGlobal(song, Filter.Local(), false)?.Entity;
        RebuildButtons();
    }

    private void StopTrailer()
    {
        if (_stream == null)
            return;

        _stream = _audio.Stop(_stream);
        RebuildButtons();
    }

    protected override void ExitedTree()
    {
        base.ExitedTree();
        StopTrailer();

        _target?.Dispose();
        _target = null;
    }

    private enum CardButton : byte
    {
        Prequel,
        Trailer,
        Guide,
        Sequels,
        Rent,
    }

    private readonly record struct ButtonEntry(
        CardButton Button,
        string Text,
        StyleBoxTexture Style,
        Font Font,
        Color Color,
        bool Enabled);
}
