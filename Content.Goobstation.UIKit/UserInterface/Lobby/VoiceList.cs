using Robust.Client.UserInterface.Controls;

namespace Content.Goobstation.UIKit.UserInterface.Lobby;

public sealed class VoiceList : BoxContainer
{
    private readonly List<(int Id, ContainerButton Row)> _items = new();

    public event Action<SegmentSelectedEventArgs>? OnItemSelected;
    public event Action<int>? OnPlayPressed;

    public int SelectedId { get; private set; } = -1;

    public VoiceList()
    {
        Orientation = LayoutOrientation.Vertical;
        SeparationOverride = 3;
        HorizontalExpand = true;
    }

    public void AddItem(string label, int id)
    {
        var row = new ContainerButton
        {
            ToggleMode = true,
            StyleClasses = { HullStyleClasses.Slot },
        };

        var content = new BoxContainer
        {
            Orientation = LayoutOrientation.Horizontal,
            SeparationOverride = 8,
        };

        var play = new Button
        {
            Text = "▶",
            StyleClasses = { HullStyleClasses.Plate },
        };
        play.OnPressed += _ => OnPlayPressed?.Invoke(id);

        content.AddChild(play);
        content.AddChild(new Label
        {
            Text = label,
            VAlign = Label.VAlignMode.Center,
            HorizontalExpand = true,
            ClipText = true,
        });
        row.AddChild(content);

        row.OnPressed += _ =>
        {
            OnItemSelected?.Invoke(new SegmentSelectedEventArgs(id));
            SelectId(SelectedId);
        };

        _items.Add((id, row));
        AddChild(row);
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
        foreach (var (itemId, row) in _items)
        {
            row.Pressed = itemId == id;
        }
    }
}
