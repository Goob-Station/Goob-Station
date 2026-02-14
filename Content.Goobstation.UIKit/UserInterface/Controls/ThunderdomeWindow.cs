using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Input;
using Robust.Shared.Maths;

namespace Content.Goobstation.UIKit.UserInterface.Controls;

public class ThunderdomeWindow : BaseWindow
{
    private const float TitleBarHeight = 36;
    private const float BorderWidth = 2;
    private const float DragMarginSize = 7;

    private readonly PanelContainer _outerBorder;
    private readonly PanelContainer _titleBar;
    private readonly Label _titleLabel;
    private readonly TextureButton _closeButton;
    private readonly PanelContainer _bodyPanel;
    private readonly BoxContainer _contentBox;

    public string WindowTitle
    {
        get => _titleLabel.Text ?? string.Empty;
        set => _titleLabel.Text = value;
    }

    public BoxContainer Contents => _contentBox;

    public ThunderdomeWindow()
    {
        MouseFilter = MouseFilterMode.Stop;
        Resizable = true;

        _outerBorder = new PanelContainer
        {
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = ThunderdomeTheme.BodyBg,
                BorderColor = ThunderdomeTheme.Border,
                BorderThickness = new Thickness(BorderWidth),
            },
        };
        AddChild(_outerBorder);

        var outerVBox = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
        };
        _outerBorder.AddChild(outerVBox);

        _titleBar = new PanelContainer
        {
            MinHeight = TitleBarHeight,
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = ThunderdomeTheme.TitleBarBg,
                BorderColor = ThunderdomeTheme.Accent,
                BorderThickness = new Thickness(0, 0, 0, 1),
            },
        };

        var titleBarContent = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            Margin = new Thickness(10, 0),
        };

        var accentBar = new PanelContainer
        {
            MinWidth = 4,
            MaxWidth = 4,
            VerticalExpand = true,
            Margin = new Thickness(0, 6, 8, 6),
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = ThunderdomeTheme.Accent,
            },
        };
        titleBarContent.AddChild(accentBar);

        _titleLabel = new Label
        {
            Text = "THUNDERDOME",
            StyleClasses = { "LabelHeadingBigger" },
            VerticalAlignment = VAlignment.Center,
            HorizontalExpand = true,
        };
        titleBarContent.AddChild(_titleLabel);

        _closeButton = new TextureButton
        {
            VerticalAlignment = VAlignment.Center,
            MinSize = new Vector2(24, 24),
        };

        var closeLabel = new Label
        {
            Text = "X",
            FontColorOverride = ThunderdomeTheme.Accent,
            VerticalAlignment = VAlignment.Center,
            HorizontalAlignment = HAlignment.Center,
        };
        _closeButton.AddChild(closeLabel);

        _closeButton.OnPressed += _ => Close();
        _closeButton.OnMouseEntered += _ => closeLabel.FontColorOverride = ThunderdomeTheme.AccentHover;
        _closeButton.OnMouseExited += _ => closeLabel.FontColorOverride = ThunderdomeTheme.Accent;

        titleBarContent.AddChild(_closeButton);
        _titleBar.AddChild(titleBarContent);
        outerVBox.AddChild(_titleBar);

        _bodyPanel = new PanelContainer
        {
            VerticalExpand = true,
            HorizontalExpand = true,
        };

        _contentBox = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true,
        };
        _bodyPanel.AddChild(_contentBox);
        outerVBox.AddChild(_bodyPanel);
    }

    protected override void Resized()
    {
        base.Resized();
        _outerBorder.SetSize = Size;
    }

    protected override DragMode GetDragModeFor(Vector2 relativeMousePos)
    {
        if (Resizable)
        {
            if (relativeMousePos.Y < DragMarginSize)
                return DragMode.Top;
            if (relativeMousePos.Y > Size.Y - DragMarginSize)
                return DragMode.Bottom;
            if (relativeMousePos.X < DragMarginSize)
                return DragMode.Left;
            if (relativeMousePos.X > Size.X - DragMarginSize)
                return DragMode.Right;
        }

        if (relativeMousePos.Y < TitleBarHeight + BorderWidth)
            return DragMode.Move;

        return DragMode.None;
    }
}
