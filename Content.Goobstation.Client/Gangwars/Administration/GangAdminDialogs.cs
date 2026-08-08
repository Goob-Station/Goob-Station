using System.Numerics;
using Content.Goobstation.Shared.Gangwars.Administration;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;

namespace Content.Goobstation.Client.Gangwars.Administration;

/// <summary>
/// Small prompt for overwriting a gang member's points.
/// </summary>
public sealed class GangAdminPointsDialog : DefaultWindow
{
    public event Action<int>? OnConfirmed;

    private readonly LineEdit _edit;

    public GangAdminPointsDialog(string memberName, int currentPoints)
    {
        Title = Loc.GetString("gang-admin-points-title", ("name", memberName));
        MinSize = new Vector2(260, 0);

        var vbox = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 8,
            Margin = new Thickness(8),
        };

        _edit = new LineEdit
        {
            Text = currentPoints.ToString(),
            HorizontalExpand = true,
        };
        _edit.OnTextEntered += _ => Confirm();
        vbox.AddChild(_edit);

        var confirm = new Button { Text = Loc.GetString("gang-admin-set-points"), HorizontalExpand = true };
        confirm.OnPressed += _ => Confirm();
        vbox.AddChild(confirm);

        Contents.AddChild(vbox);
    }

    private void Confirm()
    {
        if (!int.TryParse(_edit.Text, out var points))
            return;

        OnConfirmed?.Invoke(points);
        Close();
    }
}

/// <summary>
/// Picker listing every non-ghost player so an admin can add one to a gang.
/// </summary>
public sealed class GangAdminPlayerPickerDialog : DefaultWindow
{
    public event Action<NetEntity>? OnPicked;

    private readonly List<NetEntity> _entities = new();
    private readonly Button _addButton;
    private NetEntity? _selected;

    public GangAdminPlayerPickerDialog(List<GangAdminPlayerInfo> players)
    {
        Title = Loc.GetString("gang-admin-add-member-title");
        MinSize = new Vector2(360, 400);

        var vbox = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 6,
            Margin = new Thickness(8),
            VerticalExpand = true,
        };

        var list = new ItemList
        {
            VerticalExpand = true,
            SelectMode = ItemList.ItemListSelectMode.Single,
        };

        foreach (var player in players)
        {
            var label = player.InGang
                ? Loc.GetString("gang-admin-player-in-gang", ("name", player.Name))
                : player.Name;

            list.AddItem(label, selectable: !player.InGang);
            _entities.Add(player.Entity);
        }

        list.OnItemSelected += args =>
        {
            _selected = args.ItemIndex >= 0 && args.ItemIndex < _entities.Count
                ? _entities[args.ItemIndex]
                : null;
            UpdateAddButton();
        };
        list.OnItemDeselected += _ =>
        {
            _selected = null;
            UpdateAddButton();
        };
        vbox.AddChild(list);

        _addButton = new Button
        {
            Text = Loc.GetString("gang-admin-add-member"),
            HorizontalExpand = true,
            Disabled = true,
        };
        _addButton.OnPressed += _ =>
        {
            if (_selected is not { } selected)
                return;

            OnPicked?.Invoke(selected);
            Close();
        };
        vbox.AddChild(_addButton);

        Contents.AddChild(vbox);
    }

    private void UpdateAddButton()
    {
        _addButton.Disabled = _selected == null;
    }
}
