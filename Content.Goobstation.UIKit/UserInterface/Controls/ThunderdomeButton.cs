using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input;

namespace Content.Goobstation.UIKit.UserInterface.Controls;

public sealed class ThunderdomeButton : PanelContainer
{
    public event Action? OnPressed;

    private readonly StyleBoxFlat _styleBox;
    private readonly Label _label;
    private bool _disabled;

    public bool Disabled
    {
        get => _disabled;
        set
        {
            _disabled = value;
            UpdateVisual();
        }
    }

    public string Text
    {
        get => _label.Text ?? string.Empty;
        set => _label.Text = value;
    }

    public ThunderdomeButton()
    {
        MouseFilter = MouseFilterMode.Stop;
        HorizontalExpand = true;
        MinHeight = 45;

        _styleBox = new StyleBoxFlat
        {
            BackgroundColor = ThunderdomeTheme.ButtonBg,
            BorderColor = ThunderdomeTheme.Accent,
            BorderThickness = new Thickness(1),
            ContentMarginLeftOverride = 12,
            ContentMarginRightOverride = 12,
            ContentMarginTopOverride = 8,
            ContentMarginBottomOverride = 8,
        };
        PanelOverride = _styleBox;

        _label = new Label
        {
            Text = string.Empty,
            HorizontalAlignment = HAlignment.Center,
            VerticalAlignment = VAlignment.Center,
            HorizontalExpand = true,
            FontColorOverride = ThunderdomeTheme.ButtonText,
        };
        AddChild(_label);

        OnKeyBindDown += args =>
        {
            if (args.Function != EngineKeyFunctions.UIClick)
                return;

            if (!_disabled)
                OnPressed?.Invoke();

            args.Handle();
        };

        OnMouseEntered += _ =>
        {
            if (!_disabled)
                _styleBox.BackgroundColor = ThunderdomeTheme.ButtonHoverBg;
        };

        OnMouseExited += _ => UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (_disabled)
        {
            _styleBox.BackgroundColor = ThunderdomeTheme.ButtonDisabledBg;
            _styleBox.BorderColor = ThunderdomeTheme.AccentDim;
            _label.FontColorOverride = ThunderdomeTheme.ButtonDisabledFg;
        }
        else
        {
            _styleBox.BackgroundColor = ThunderdomeTheme.ButtonBg;
            _styleBox.BorderColor = ThunderdomeTheme.Accent;
            _label.FontColorOverride = ThunderdomeTheme.ButtonText;
        }
    }
}
