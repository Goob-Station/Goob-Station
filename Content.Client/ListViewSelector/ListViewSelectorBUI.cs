using Content.Client.Lathe.UI;
using Content.Client.UserInterface.Controls;
using Content.Shared.ListViewSelector;
using JetBrains.Annotations;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;

// ReSharper disable InconsistentNaming

namespace Content.Client.ListViewSelector;

[UsedImplicitly]
public sealed class ListViewSelectorBUI(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private FancyWindow _window = new();
    private BoxContainer? _itemsContainer;
    private Dictionary<string, object> _metaData = new();

    protected override void Open()
    {
        _window = FormWindow();
        _window.OnClose += Close;
        _window.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is not ListViewSelectorState listViewSelectorState)
            return;

        PopulateWindow(listViewSelectorState.Items);
        _metaData = listViewSelectorState.MetaData;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
            _window.Close();
    }

    private FancyWindow FormWindow()
    {
        var window = new FancyWindow
        {
            HorizontalExpand = true,
            VerticalExpand = true,
            MinWidth = 350,
            MinHeight = 400,
            Title = Loc.GetString("list-view-window-default-title")
        };

        var scrollContainer = new ScrollContainer
        {
            HorizontalExpand = true,
            VerticalExpand = true
        };

        var itemsContainer = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical
        };

        scrollContainer.AddChild(itemsContainer);
        window.AddChild(scrollContainer);

        _itemsContainer = itemsContainer;

        return window;
    }

    private void PopulateWindow(List<ListViewSelectorEntry> items)
    {
        if (_itemsContainer is null)
            return;

        _itemsContainer.Children.Clear();

        foreach (var item in items)
        {
            var itemName = item.Name;
            var itemDesc = item.Description;
            if (_prototypeManager.TryIndex(item.Id, out var itemPrototype, false))
            {
                itemName = itemPrototype.Name;
                itemDesc = itemPrototype.Description;
            }

            var button = new Button
            {
                Text = itemName,
            };

            if (!string.IsNullOrEmpty(itemDesc))
                button.TooltipSupplier = _ => new RecipeTooltip(itemDesc);

            button.OnButtonUp += _ =>
            {
                var msg = new ListViewItemSelectedMessage(item, items.IndexOf(item), _metaData);
                SendMessage(msg);
                Close();
            };

            _itemsContainer.AddChild(button);
        }
    }
}
