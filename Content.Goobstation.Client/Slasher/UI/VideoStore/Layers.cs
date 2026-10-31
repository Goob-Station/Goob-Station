using Robust.Client.UserInterface.Controls;

namespace Content.Goobstation.Client.Slasher.UI.VideoStore;

public sealed class Layers : BoxContainer
{
    public event Action<int>? OnJump;

    public Layers()
    {
        Orientation = LayoutOrientation.Horizontal;
        SeparationOverride = 6;
    }

    public void Set(IReadOnlyList<string> names)
    {
        DisposeAllChildren();

        AddButton(Loc.GetString("slasher-kit-store-originals"), 0, names.Count == 0);
        for (var i = 0; i < names.Count; i++)
        {
            var arrow = new Label { Text = ">", VerticalAlignment = VAlignment.Center };
            arrow.AddStyleClass(StoreButtonStyles.LayerArrow);
            AddChild(arrow);
            AddButton(names[i].ToUpperInvariant(), i + 1, i == names.Count - 1);
        }
    }

    private void AddButton(string text, int layer, bool current)
    {
        var button = new Button { Text = text };
        button.AddStyleClass(current ? StoreButtonStyles.LayerCurrent : StoreButtonStyles.Layer);
        button.OnPressed += _ => OnJump?.Invoke(layer);
        AddChild(button);
    }
}
