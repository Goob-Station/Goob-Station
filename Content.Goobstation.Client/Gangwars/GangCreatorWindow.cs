using System.Linq;
using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;

namespace Content.Goobstation.Client.Gangwars;

public sealed class GangCreatorWindow : DefaultWindow
{
    public event Action<Color, string>? OnColorPicked;

    private readonly ColorSelectorSliders _colorSelector;
    private readonly PanelContainer _previewSwatch;
    private readonly IReadOnlyDictionary<Color, string> _gangNames;
    private const float MinAlpha = 0.9f;
    private const float MinValue = 0.70f;
    private const float SimilarityThreshold = 0.15f;
    private const int MinNameLength = 4;
    private const int MaxNameLength = 17;

    public GangCreatorWindow(IReadOnlyDictionary<Color, string> gangNames, Color? initialColor = null, string? initialName = null)
    {
        _gangNames = gangNames;

        Title = Loc.GetString("gang-creator-title");
        MinSize = new Vector2(320, 340);

        var vbox = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 6,
            Margin = new Thickness(4),
        };

        var colorLabel = new Label
        {
            Text = Loc.GetString("gang-creator-color-label"),
            Margin = new Thickness(0, 0, 0, 4),
        };
        vbox.AddChild(colorLabel);

        _colorSelector = new ColorSelectorSliders
        {
            SelectorType = ColorSelectorSliders.ColorSelectorType.Hsv,
            Color = ClampColor(initialColor ?? Color.White),
        };
        vbox.AddChild(_colorSelector);

        _previewSwatch = new PanelContainer
        {
            MinSize = new Vector2(0, 32),
            HorizontalExpand = true,
            PanelOverride = new StyleBoxFlat { BackgroundColor = _colorSelector.Color },
            Margin = new Thickness(0, 4),
        };
        vbox.AddChild(_previewSwatch);

        var tooSimilarWarning = new Label
        {
            Text = Loc.GetString("gang-color-already-taken"),
            FontColorOverride = Color.Red,
            Visible = IsColorTooSimilarToClaimed(_colorSelector.Color),
        };
        vbox.AddChild(tooSimilarWarning);

        vbox.AddChild(new Label
        {
            Text = Loc.GetString("gang-creator-name-label"),
            Margin = new Thickness(0, 4, 0, 0),
        });

        var nameInput = new LineEdit
        {
            PlaceHolder = Loc.GetString("gang-creator-name-placeholder"),
            HorizontalExpand = true,
            Text = initialName ?? string.Empty,
        };
        vbox.AddChild(nameInput);

        var namWarningLabel = new Label
        {
            FontColorOverride = Color.Red,
            Visible = false,
        };
        vbox.AddChild(namWarningLabel);

        var confirmBtn = new Button
        {
            Text = Loc.GetString("gang-creator-confirm"),
            HorizontalExpand = true,
            Disabled = true,
        };

        void UpdateConfirmState()
        {
            var colorBad = IsColorTooSimilarToClaimed(_colorSelector.Color);
            var name = nameInput.Text.Trim();
            var nameTaken = IsNameTaken(name);
            var nameBad = name.Length < MinNameLength || name.Length > MaxNameLength || nameTaken;
            confirmBtn.Disabled = colorBad || nameBad;
        }

        _colorSelector.OnColorChanged += color =>
        {
            var clamped = ClampColor(color);
            if (clamped != color)
            {
                _colorSelector.Color = clamped;
                return;
            }

            _previewSwatch.PanelOverride = new StyleBoxFlat { BackgroundColor = color };
            tooSimilarWarning.Visible = IsColorTooSimilarToClaimed(color);
            UpdateConfirmState();
        };

        nameInput.OnTextChanged += args =>
        {
            var name = args.Text.Trim();
            if (name.Length < MinNameLength)
            {
                namWarningLabel.Text = Loc.GetString("gang-creator-name-too-short", ("min", MinNameLength));
                namWarningLabel.Visible = true;
            }
            else if (name.Length > MaxNameLength)
            {
                namWarningLabel.Text = Loc.GetString("gang-creator-name-too-long", ("max", MaxNameLength));
                namWarningLabel.Visible = true;
            }
            else if (IsNameTaken(name))
            {
                namWarningLabel.Text = Loc.GetString("gang-creator-name-already-taken");
                namWarningLabel.Visible = true;
            }
            else
            {
                namWarningLabel.Visible = false;
            }
            UpdateConfirmState();
        };

        confirmBtn.OnPressed += _ =>
        {
            var name = nameInput.Text.Trim();
            if (IsColorTooSimilarToClaimed(_colorSelector.Color) || name.Length < MinNameLength || name.Length > MaxNameLength || IsNameTaken(name))
                return;

            OnColorPicked?.Invoke(_colorSelector.Color, name);
            Close();
        };
        vbox.AddChild(confirmBtn);

        UpdateConfirmState();

        Contents.AddChild(vbox);
    }

    private static Color ClampColor(Color color)
    {
        var hsv = Color.ToHsv(color);
        var clampedValue = MathF.Max(hsv.Z, MinValue);
        var clampedAlpha = MathF.Max(color.A, MinAlpha);
        return Color.FromHsv(new Vector4(hsv.X, hsv.Y, clampedValue, clampedAlpha));
    }

    private bool IsNameTaken(string name)
        => _gangNames.Values.Any(n => n.Equals(name, StringComparison.OrdinalIgnoreCase));

    private bool IsColorTooSimilarToClaimed(Color color)
    {
        foreach (var claimed in _gangNames.Keys)
        {
            var redDiff = color.R - claimed.R;
            var greenDiff = color.G - claimed.G;
            var blueDiff = color.B - claimed.B;
            if (MathF.Sqrt(redDiff * redDiff + greenDiff * greenDiff + blueDiff * blueDiff) < SimilarityThreshold)
                return true;
        }
        return false;
    }
}
