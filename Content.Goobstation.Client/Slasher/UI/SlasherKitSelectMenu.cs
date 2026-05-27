using Content.Client.UserInterface.Controls;
using Content.Goobstation.Shared.Slasher.UI;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;

namespace Content.Goobstation.Client.Slasher.UI;

/// <summary>
/// Horizontal kit selection window for the Slasher.
/// </summary>
public sealed class SlasherKitSelectMenu : FancyWindow
{
    [Dependency] private readonly SpriteSystem _spriteSystem = default!;

    private readonly BoxContainer _kitsContainer;

    public event Action<int>? OnKitSelected;

    public SlasherKitSelectMenu()
    {
        IoCManager.InjectDependencies(this);

        Title = Loc.GetString("slasher-kit-select-title");
        MinSize = new System.Numerics.Vector2(500, 420);

        CloseButton.Visible = false;

        var root = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true
        };

        var header = new Label
        {
            Text = Loc.GetString("slasher-kit-select-header"),
            Margin = new Thickness(5)
        };

        var hline = new PanelContainer
        {
            Margin = new Thickness(0, 5),
            MinHeight = 2,
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#404040")
            }
        };

        var scrollPanel = new PanelContainer
        {
            Margin = new Thickness(5),
            VerticalExpand = true,
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#050505")
            }
        };

        var scroll = new ScrollContainer
        {
            HScrollEnabled = true,
            VScrollEnabled = false,
            VerticalExpand = true
        };

        _kitsContainer = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            VerticalExpand = true,
            SeparationOverride = 4,
            Margin = new Thickness(4)
        };
        root.AddChild(header);
        root.AddChild(hline);
        scroll.AddChild(_kitsContainer);
        scrollPanel.AddChild(scroll);
        root.AddChild(scrollPanel);

        ContentsContainer.AddChild(root);
    }

    // Block the close button — player must pick a kit.
    public override void Close() { }
    public void ForceClose() => base.Close();

    public void UpdateState(SlasherKitSelectBoundUserInterfaceState state)
    {
        _kitsContainer.DisposeAllChildren();

        for (var i = 0; i < state.Kits.Count; i++)
        {
            var index = i;
            var kit = state.Kits[i];
            var card = new SlasherKitCard(kit, _spriteSystem);
            card.SelectButton.OnPressed += _ => OnKitSelected?.Invoke(index);
            _kitsContainer.AddChild(card);
        }
    }
}
