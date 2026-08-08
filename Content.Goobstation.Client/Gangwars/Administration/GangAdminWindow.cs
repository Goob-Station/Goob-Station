using System.Linq;
using System.Numerics;
using Content.Client.UserInterface.Controls;
using Content.Goobstation.Shared.Gangwars.Administration;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;

namespace Content.Goobstation.Client.Gangwars.Administration;

/// <summary>
/// Admin Panel for the gangwars gamemode. 
/// </summary>
public sealed class GangAdminWindow : DefaultWindow
{
    public event Action<Color, Color, string>? OnManualRename;
    public event Action<Color>? OnForceRename;
    public event Action<NetEntity>? OnKick;
    public event Action<NetEntity>? OnSetLeader;
    public event Action<NetEntity, int>? OnSetPoints;
    public event Action<Color, NetEntity>? OnAddMember;

    private readonly BoxContainer _gangList;
    private readonly Label _detailsTitle;
    private readonly BoxContainer _memberList;
    private readonly Button _manualRenameButton;
    private readonly ConfirmButton _forceRenameButton;
    private readonly ConfirmButton _kickButton;
    private readonly ConfirmButton _setLeaderButton;
    private readonly Button _addMemberButton;

    private List<GangAdminInfo> _gangs = new();
    private List<GangAdminPlayerInfo> _players = new();
    private Color? _selectedColor;
    private NetEntity? _selectedMember;

    private GangCreatorWindow? _renameWindow;
    private GangAdminPointsDialog? _pointsDialog;
    private GangAdminPlayerPickerDialog? _pickerDialog;

    public GangAdminWindow()
    {
        Title = Loc.GetString("gang-admin-title");
        MinSize = new Vector2(850, 420);

        var split = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 8,
            Margin = new Thickness(4),
            VerticalExpand = true,
        };

        var leftScroll = new ScrollContainer
        {
            HScrollEnabled = false,
            MinSize = new Vector2(200, 0),
            VerticalExpand = true,
        };
        _gangList = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 2,
            HorizontalExpand = true,
        };
        leftScroll.AddChild(_gangList);
        split.AddChild(leftScroll);

        var right = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 6,
            HorizontalExpand = true,
            VerticalExpand = true,
        };

        _detailsTitle = new Label
        {
            Text = Loc.GetString("gang-admin-no-selection"),
            StyleClasses = { "LabelHeading" },
        };
        right.AddChild(_detailsTitle);

        right.AddChild(new Label { Text = Loc.GetString("gang-admin-members-header") });

        var memberScroll = new ScrollContainer
        {
            HScrollEnabled = false,
            VerticalExpand = true,
        };
        _memberList = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 2,
            HorizontalExpand = true,
        };
        memberScroll.AddChild(_memberList);
        right.AddChild(memberScroll);

        var buttonRow = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 4,
            HorizontalExpand = true,
        };

        _manualRenameButton = new Button { Text = Loc.GetString("gang-admin-manual-rename"), HorizontalExpand = true };
        _manualRenameButton.OnPressed += _ => OpenManualRename();

        _forceRenameButton = new ConfirmButton
        {
            Text = Loc.GetString("gang-admin-force-rename"),
            ConfirmationText = Loc.GetString("gang-admin-force-rename"),
            HorizontalExpand = true,
        };
        _forceRenameButton.OnPressed += _ =>
        {
            if (_selectedColor is { } color)
                OnForceRename?.Invoke(color);
        };

        _kickButton = new ConfirmButton
        {
            Text = Loc.GetString("gang-admin-kick"),
            ConfirmationText = Loc.GetString("gang-admin-kick"),
            HorizontalExpand = true,
        };
        _kickButton.OnPressed += _ =>
        {
            if (_selectedMember is { } member)
                OnKick?.Invoke(member);
        };

        _setLeaderButton = new ConfirmButton
        {
            Text = Loc.GetString("gang-admin-set-leader"),
            ConfirmationText = Loc.GetString("gang-admin-set-leader"),
            HorizontalExpand = true,
        };
        _setLeaderButton.OnPressed += _ =>
        {
            if (_selectedMember is { } member)
                OnSetLeader?.Invoke(member);
        };

        _addMemberButton = new Button { Text = Loc.GetString("gang-admin-add-member"), HorizontalExpand = true };
        _addMemberButton.OnPressed += _ => OpenAddMember();

        buttonRow.AddChild(_manualRenameButton);
        buttonRow.AddChild(_forceRenameButton);
        buttonRow.AddChild(_addMemberButton);
        buttonRow.AddChild(_kickButton);
        buttonRow.AddChild(_setLeaderButton);
        right.AddChild(buttonRow);

        split.AddChild(right);
        Contents.AddChild(split);

        UpdateButtons();
    }

    public void SetData(List<GangAdminInfo> gangs, List<GangAdminPlayerInfo> players)
    {
        _gangs = gangs;
        _players = players;

        if (_selectedColor is { } selected && _gangs.All(g => g.Color != selected))
            _selectedColor = null;

        RebuildGangList();
        RebuildDetails();
    }

    private void RebuildGangList()
    {
        _gangList.RemoveAllChildren();

        foreach (var gang in _gangs)
        {
            var color = gang.Color;
            var row = new BoxContainer
            {
                Orientation = BoxContainer.LayoutOrientation.Horizontal,
                SeparationOverride = 4,
                HorizontalExpand = true,
            };

            row.AddChild(new PanelContainer
            {
                MinSize = new Vector2(16, 16),
                VerticalAlignment = VAlignment.Center,
                PanelOverride = new StyleBoxFlat { BackgroundColor = color },
            });

            var button = new Button
            {
                Text = gang.Name,
                HorizontalExpand = true,
                ToggleMode = true,
                Pressed = _selectedColor == color,
            };
            button.OnPressed += _ =>
            {
                _selectedColor = color;
                ClearMemberSelection();
                RebuildGangList();
                RebuildDetails();
            };

            row.AddChild(button);
            _gangList.AddChild(row);
        }
    }

    private void RebuildDetails()
    {
        _memberList.RemoveAllChildren();

        if (_selectedColor is not { } color || _gangs.All(g => g.Color != color))
        {
            ClearMemberSelection();
            _detailsTitle.Text = Loc.GetString("gang-admin-no-selection");
            _detailsTitle.FontColorOverride = null;
            UpdateButtons();
            return;
        }

        var gang = _gangs.First(g => g.Color == color);
        _detailsTitle.Text = gang.Name;
        _detailsTitle.FontColorOverride = color;

        if (_selectedMember is { } selectedMember && gang.Members.All(m => m.Entity != selectedMember))
            ClearMemberSelection();

        foreach (var member in gang.Members.OrderByDescending(m => m.IsLeader))
            _memberList.AddChild(BuildMemberRow(member));

        UpdateButtons();
    }

    private BoxContainer BuildMemberRow(GangAdminMemberInfo member)
    {
        var row = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 4,
            HorizontalExpand = true,
        };

        var nameText = member.IsLeader
            ? Loc.GetString("gang-admin-member-leader", ("name", member.Name))
            : member.Name;

        var nameButton = new Button
        {
            Text = nameText,
            HorizontalExpand = true,
            ToggleMode = true,
            Pressed = _selectedMember == member.Entity,
        };
        nameButton.OnPressed += _ =>
        {
            _selectedMember = member.Entity;
            RebuildDetails();
        };
        row.AddChild(nameButton);

        var pointsButton = new Button
        {
            Text = Loc.GetString("gang-admin-points-button", ("points", member.Points)),
        };
        pointsButton.OnPressed += _ => OpenPointsDialog(member);
        row.AddChild(pointsButton);

        return row;
    }

    private void OpenPointsDialog(GangAdminMemberInfo member)
    {
        _pointsDialog?.Close();
        _pointsDialog = new GangAdminPointsDialog(member.Name, member.Points);
        _pointsDialog.OnConfirmed += points => OnSetPoints?.Invoke(member.Entity, points);
        _pointsDialog.OpenCentered();
    }

    private void OpenAddMember()
    {
        if (_selectedColor is not { } color)
            return;

        _pickerDialog?.Close();
        _pickerDialog = new GangAdminPlayerPickerDialog(_players);
        _pickerDialog.OnPicked += player => OnAddMember?.Invoke(color, player);
        _pickerDialog.OpenCentered();
    }

    private void OpenManualRename()
    {
        if (_selectedColor is not { } oldColor || _gangs.All(g => g.Color != oldColor))
            return;

        var gang = _gangs.First(g => g.Color == oldColor);

        _renameWindow?.Close();
        _renameWindow = new GangCreatorWindow(new Dictionary<Color, string>(), oldColor, gang.Name);
        _renameWindow.OnColorPicked += (newColor, name) => OnManualRename?.Invoke(oldColor, newColor, name);
        _renameWindow.OpenCentered();
    }

    private void ClearMemberSelection()
    {
        _selectedMember = null;
    }

    private void UpdateButtons()
    {
        var hasGang = _selectedColor != null;
        var hasMember = _selectedMember != null;

        _manualRenameButton.Disabled = !hasGang;
        _forceRenameButton.Disabled = !hasGang;
        _addMemberButton.Disabled = !hasGang;
        _kickButton.Disabled = !hasMember;
        _setLeaderButton.Disabled = !hasMember;
    }
}
