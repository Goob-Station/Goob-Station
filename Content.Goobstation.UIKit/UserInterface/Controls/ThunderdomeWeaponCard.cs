using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.UIKit.UserInterface.Controls;

public sealed class ThunderdomeWeaponCard : PanelContainer
{
    public event Action<ThunderdomeWeaponCard>? OnSelected;

    public int WeaponIndex { get; }

    private bool _isSelected;
    private readonly StyleBoxFlat _styleBox;

    public ThunderdomeWeaponCard(int weaponIndex, string weaponName, string? spritePrototype, string? tooltip = null)
    {
        WeaponIndex = weaponIndex;
        MouseFilter = MouseFilterMode.Stop;

        if (!string.IsNullOrEmpty(tooltip))
        {
            TooltipSupplier = _ =>
            {
                var panel = new PanelContainer
                {
                    PanelOverride = new StyleBoxFlat
                    {
                        BackgroundColor = ThunderdomeTheme.TitleBarBg,
                        BorderColor = ThunderdomeTheme.Accent,
                        BorderThickness = new Thickness(1),
                        ContentMarginLeftOverride = 8,
                        ContentMarginRightOverride = 8,
                        ContentMarginTopOverride = 4,
                        ContentMarginBottomOverride = 4,
                    },
                };

                var label = new Label
                {
                    Text = tooltip,
                    FontColorOverride = ThunderdomeTheme.ButtonText,
                };
                panel.AddChild(label);

                return panel;
            };
        }

        _styleBox = new StyleBoxFlat
        {
            BackgroundColor = ThunderdomeTheme.CardBg,
            ContentMarginLeftOverride = 8,
            ContentMarginRightOverride = 8,
            ContentMarginTopOverride = 6,
            ContentMarginBottomOverride = 6,
            BorderColor = Color.Transparent,
            BorderThickness = new Thickness(2),
        };
        PanelOverride = _styleBox;

        HorizontalExpand = true;
        MinHeight = 56;

        var hbox = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            HorizontalExpand = true,
            VerticalExpand = true,
        };

        if (!string.IsNullOrEmpty(spritePrototype))
        {
            var spriteView = new EntityPrototypeView
            {
                MinSize = new Vector2(48, 48),
                SetSize = new Vector2(48, 48),
                Stretch = SpriteView.StretchMode.Fit,
                HorizontalAlignment = HAlignment.Center,
                VerticalAlignment = VAlignment.Center,
            };
            spriteView.SetPrototype(new EntProtoId(spritePrototype));
            hbox.AddChild(spriteView);
        }

        var nameLabel = new Label
        {
            Text = weaponName,
            StyleClasses = { "LabelHeading" },
            VerticalAlignment = VAlignment.Center,
            HorizontalExpand = true,
            Margin = new Thickness(8, 0, 0, 0),
        };
        hbox.AddChild(nameLabel);

        AddChild(hbox);

        OnKeyBindDown += args =>
        {
            if (args.Function != EngineKeyFunctions.UIClick)
                return;

            OnSelected?.Invoke(this);
            args.Handle();
        };

        OnMouseEntered += _ =>
        {
            if (!_isSelected)
                _styleBox.BackgroundColor = ThunderdomeTheme.CardHoverBg;
        };

        OnMouseExited += _ =>
        {
            if (!_isSelected)
                _styleBox.BackgroundColor = ThunderdomeTheme.CardBg;
        };
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        _styleBox.BorderColor = selected ? ThunderdomeTheme.Border : Color.Transparent;
        _styleBox.BackgroundColor = selected ? ThunderdomeTheme.CardHoverBg : ThunderdomeTheme.CardBg;
    }
}
