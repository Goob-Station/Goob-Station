using Robust.Client.UserInterface.Controls;

namespace Content.Goobstation.UIKit.UserInterface.Lobby;

public sealed class SegmentedOptions : BoxContainer
{
    private readonly List<(int Id, Button Button)> _items = new();

    public event Action<SegmentSelectedEventArgs>? OnItemSelected;

    public int SelectedId { get; private set; } = -1;

    public SegmentedOptions()
    {
        Orientation = LayoutOrientation.Horizontal;
        SeparationOverride = 2;
    }

    public void AddItem(string label, int id)
    {
        var button = new Button
        {
            Text = label,
            ToggleMode = true,
            StyleClasses = { HullStyleClasses.Tab },
        };

        button.OnPressed += _ =>
        {
            OnItemSelected?.Invoke(new SegmentSelectedEventArgs(id));
            SelectId(SelectedId);
        };

        _items.Add((id, button));
        AddChild(button);
    }

    public void Clear()
    {
        _items.Clear();
        RemoveAllChildren();
        SelectedId = -1;
    }

    public void SelectId(int id)
    {
        SelectedId = id;
        foreach (var (itemId, button) in _items)
        {
            button.Pressed = itemId == id;
        }
    }
}

public sealed class SegmentSelectedEventArgs(int id) : EventArgs
{
    public int Id { get; } = id;
}
