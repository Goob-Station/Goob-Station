using Content.Client.UserInterface.Controls;
using Content.Goobstation.Shared.Doodon.Building;
using Robust.Client.UserInterface;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.Doodon.Building;

public sealed class DoodonBuildMenuBoundUserInterface : BoundUserInterface
{
    private SimpleRadialMenu? _menu;

    // client-only model for the generic radial option
    private sealed class DoodonBuildRadialItem
    {
        public string PrototypeId { get; }
        public string Name { get; }
        public SpriteSpecifier? Icon { get; }

        public DoodonBuildRadialItem(string prototypeId, string name, SpriteSpecifier? icon)
        {
            PrototypeId = prototypeId;
            Name = name;
            Icon = icon;
        }
    }

    public DoodonBuildMenuBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<SimpleRadialMenu>();
        _menu.Track(Owner);
        _menu.OpenOverMouseScreenPosition();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_menu == null || state is not DoodonBuildMenuState s)
            return;

        var models = new RadialMenuOption[s.Entries.Length];

        for (var i = 0; i < s.Entries.Length; i++)
        {
            var e = s.Entries[i];
            var item = new DoodonBuildRadialItem(e.PrototypeId, e.Name, e.Icon);

            var costText = e.ResinCost > 0 ? $"{e.ResinCost} resin" : "Free";
            var tooltip = $"{e.Name} ({costText})";

            if (!string.IsNullOrWhiteSpace(e.Description))
                tooltip += $"\n{e.Description}";

            models[i] = new RadialMenuActionOption<DoodonBuildRadialItem>(HandleClick, item)
            {
                Sprite = e.Icon,
                ToolTip = tooltip
            };
        }

        _menu.SetButtons(models);
    }

    private void HandleClick(DoodonBuildRadialItem item)
    {
        // non-predicted: menu closes immediately
        SendMessage(new DoodonBuildSelectMessage(item.PrototypeId));
        Close();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
            return;

        _menu?.Dispose();
        _menu = null;
    }
}
