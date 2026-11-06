using System.Numerics;
using Content.Goobstation.UIKit.UserInterface.Lobby;
using Content.Shared.Humanoid.Prototypes;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Map;

namespace Content.Goobstation.UIKit.UserInterface.Humanoid;

public sealed partial class SpeciesGallery : BoxContainer
{
    [Dependency] private IEntityManager _entMan = default!;

    private const float TileWidth = 92;

    private readonly TileFlowContainer _grid;
    private readonly List<(int Id, ContainerButton Button)> _items = new();
    private readonly List<EntityUid> _dolls = new();
    private IReadOnlyList<SpeciesPrototype> _species = Array.Empty<SpeciesPrototype>();

    public event Action<SegmentSelectedEventArgs>? OnItemSelected;

    public int SelectedId { get; private set; } = -1;

    public SpeciesGallery()
    {
        IoCManager.InjectDependencies(this);
        HorizontalExpand = true;

        _grid = new TileFlowContainer
        {
            HorizontalExpand = true,
        };
        AddChild(_grid);
    }

    public void SetSpeciesList(IReadOnlyList<SpeciesPrototype> species)
    {
        _species = species;
    }

    public void AddItem(string label, int id)
    {
        var tile = new ContainerButton
        {
            ToggleMode = true,
            SetWidth = TileWidth,
            StyleClasses = { HullStyleClasses.Slot },
            ToolTip = label,
        };

        var column = new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            SeparationOverride = 2,
        };

        if (id >= 0 && id < _species.Count)
        {
            var doll = _entMan.SpawnEntity(_species[id].DollPrototype, MapCoordinates.Nullspace);
            _dolls.Add(doll);

            var view = new SpriteView
            {
                Scale = new Vector2(2, 2),
                SetSize = new Vector2(64, 64),
                OverrideDirection = Direction.South,
                HorizontalAlignment = HAlignment.Center,
                MouseFilter = MouseFilterMode.Ignore,
            };
            view.SetEntity(doll);
            column.AddChild(view);
        }

        column.AddChild(new Label
        {
            Text = label,
            ClipText = true,
            Align = Label.AlignMode.Center,
            StyleClasses = { HullStyleClasses.PostDate },
        });
        tile.AddChild(column);

        tile.OnPressed += _ =>
        {
            OnItemSelected?.Invoke(new SegmentSelectedEventArgs(id));
            Refresh();
        };

        _items.Add((id, tile));
        _grid.AddChild(tile);
    }

    public void Clear()
    {
        _items.Clear();
        _grid.RemoveAllChildren();
        DeleteDolls();
        SelectedId = -1;
    }

    public void SelectId(int id)
    {
        SelectedId = id;
        Refresh();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
            DeleteDolls();
    }

    private void Refresh()
    {
        foreach (var (id, button) in _items)
        {
            button.Pressed = id == SelectedId;
        }
    }

    private void DeleteDolls()
    {
        foreach (var doll in _dolls)
        {
            _entMan.DeleteEntity(doll);
        }

        _dolls.Clear();
    }
}
