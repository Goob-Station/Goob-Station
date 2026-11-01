using Content.Goobstation.UIKit.UserInterface.Lobby;

namespace Content.Client.Lobby.UI.Roles;

public sealed partial class RequirementsSelector
{
    private readonly Dictionary<int, TintButton> _buttons = new();
    private Dictionary<int, Color>? _selectedColors;

    public Dictionary<int, Color>? SelectedColors
    {
        get => _selectedColors;
        set
        {
            _selectedColors = value;
            UpdateSelectedTint();
        }
    }

    private void UpdateSelectedTint()
    {
        foreach (var (value, button) in _buttons)
        {
            button.Tint = value == Selected && _selectedColors != null && _selectedColors.TryGetValue(value, out var color)
                ? color
                : null;
        }
    }
}
