using Content.Goobstation.Shared.MisandryBox.Thunderdome;
using Content.Goobstation.UIKit.UserInterface.Controls;
using Robust.Client.UserInterface.Controls;

namespace Content.Goobstation.Client.MisandryBox.Thunderdome;

public sealed class ThunderdomeLoadoutWindow : ThunderdomeWindow
{
    public event Action<int>? OnLoadoutConfirmed;

    private int _weaponSelection = -1;
    private ThunderdomeWeaponCard? _selectedCard;

    private readonly Label _playerCountLabel;
    private readonly BoxContainer _categoriesContainer;
    private readonly ThunderdomeButton _confirmButton;

    public ThunderdomeLoadoutWindow()
    {
        WindowTitle = Loc.GetString("thunderdome-loadout-title");
        SetSize = new System.Numerics.Vector2(450, 500);

        _playerCountLabel = new Label
        {
            Text = Loc.GetString("thunderdome-loadout-players", ("count", 0)),
            StyleClasses = { "LabelSubText" },
            HorizontalAlignment = HAlignment.Center,
            Margin = new Thickness(0, 4, 0, 4),
        };
        Contents.AddChild(_playerCountLabel);

        var subtitle = new Label
        {
            Text = Loc.GetString("thunderdome-loadout-subtitle"),
            StyleClasses = { "LabelSubText" },
            HorizontalAlignment = HAlignment.Center,
            Margin = new Thickness(0, 0, 0, 6),
        };
        Contents.AddChild(subtitle);

        _categoriesContainer = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            SeparationOverride = 4,
        };

        var scroll = new ScrollContainer
        {
            VerticalExpand = true,
            HorizontalExpand = true,
            Margin = new Thickness(6, 0),
        };
        scroll.AddChild(_categoriesContainer);
        Contents.AddChild(scroll);

        _confirmButton = new ThunderdomeButton
        {
            Text = Loc.GetString("thunderdome-loadout-confirm"),
            Disabled = true,
            Margin = new Thickness(8, 6),
        };
        _confirmButton.OnPressed += () =>
        {
            if (_weaponSelection >= 0)
                OnLoadoutConfirmed?.Invoke(_weaponSelection);
        };
        Contents.AddChild(_confirmButton);
    }

    public void UpdateState(ThunderdomeLoadoutEuiState state)
    {
        _playerCountLabel.Text = Loc.GetString("thunderdome-loadout-players", ("count", state.PlayerCount));

        _categoriesContainer.RemoveAllChildren();
        _selectedCard = null;
        _weaponSelection = -1;
        _confirmButton.Disabled = true;

        var categories = new List<(string Category, List<ThunderdomeLoadoutOption> Options)>();
        var categoryMap = new Dictionary<string, List<ThunderdomeLoadoutOption>>();

        foreach (var option in state.Weapons)
        {
            if (!categoryMap.TryGetValue(option.Category, out var list))
            {
                list = new List<ThunderdomeLoadoutOption>();
                categoryMap[option.Category] = list;
                categories.Add((option.Category, list));
            }
            list.Add(option);
        }

        foreach (var (category, options) in categories)
        {
            var header = new Label
            {
                Text = category,
                StyleClasses = { "LabelKeyText" },
                Margin = new Thickness(4, 6, 0, 2),
            };
            _categoriesContainer.AddChild(header);

            foreach (var option in options)
            {
                var card = new ThunderdomeWeaponCard(option.Index, option.Name, option.SpritePrototype, option.Description);
                card.OnSelected += OnCardSelected;
                _categoriesContainer.AddChild(card);
            }
        }
    }

    private void OnCardSelected(ThunderdomeWeaponCard card)
    {
        _selectedCard?.SetSelected(false);

        _selectedCard = card;
        _weaponSelection = card.WeaponIndex;
        card.SetSelected(true);

        _confirmButton.Disabled = false;
    }
}
