using System.Numerics;
using Content.Client.Construction;
using Content.Client.UserInterface.Controls;
using Content.Goobstation.Shared.ShortConstruction;
using Content.Shared.Construction.Prototypes;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Placement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

// ReSharper disable InconsistentNaming

namespace Content.Goobstation.Client.BloodCult.ShortConstruction.UI;

[UsedImplicitly]
public sealed class ShortConstructionMenuBUI : BoundUserInterface
{
    [Dependency] private readonly IClyde _displayManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly EntityManager _entManager = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly IPlacementManager _placementManager = default!;

    private readonly ConstructionSystem _construction;
    private readonly SpriteSystem _spriteSystem;

    private RadialMenu? _menu;

    public ShortConstructionMenuBUI(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        _construction = _entManager.System<ConstructionSystem>();
        _spriteSystem = _entManager.System<SpriteSystem>();
    }

    protected override void Open()
    {
        base.Open();

        _menu = new RadialMenu
        {
            HorizontalExpand = true,
            VerticalExpand = true,
            BackButtonStyleClass = "RadialMenuBackButton",
            CloseButtonStyleClass = "RadialMenuCloseButton"
        };

        if (_entManager.TryGetComponent<ShortConstructionComponent>(Owner, out var crafting))
            CreateMenu(crafting.Entries);

        _menu.OpenCenteredAt(_inputManager.MouseScreenPosition.Position / _displayManager.ScreenSize);
    }

    private void CreateMenu(List<ShortConstructionEntry> entries, string? parentCategory = null)
    {
        if (_menu == null)
            return;

        var container = new RadialContainer
        {
            Name = parentCategory ?? "Main",
            InitialRadius = 48f + 24f * MathF.Log(entries.Count),
        };

        _menu.AddChild(container);

        foreach (var entry in entries)
        {
            if (entry.Category != null)
            {
                var button = CreateButton(entry.Category.Name, _spriteSystem.Frame0(entry.Category.Icon));
                button.TargetLayerControlName = entry.Category.Name;
                CreateMenu(entry.Category.Entries, entry.Category.Name);
                container.AddChild(button);
            }
            else if (entry.Prototype != null && _protoManager.TryIndex(entry.Prototype, out var proto) &&
                     proto.Name != null && _construction.TryGetRecipePrototype(proto.ID, out var targetProtoId) &&
                     _protoManager.TryIndex(targetProtoId, out EntityPrototype? entProto))
            {
                var button = CreateButton(proto.Name, _spriteSystem.GetPrototypeIcon(entProto).Default);
                button.OnButtonUp += _ => ConstructItem(proto);
                container.AddChild(button);
            }
        }
    }

    private RadialMenuTextureButton CreateButton(string name, Texture icon)
    {
        var button = new RadialMenuTextureButton
        {
            ToolTip = Loc.GetString(name),
            StyleClasses = { "RadialMenuButton" },
            SetSize = new Vector2(64f, 64f),
        };

        var texture = new TextureRect
        {
            VerticalAlignment = Control.VAlignment.Center,
            HorizontalAlignment = Control.HAlignment.Center,
            Texture = icon,
            TextureScale = new Vector2(2f, 2f)
        };

        button.AddChild(texture);
        return button;
    }

    /// <summary>
    /// Makes an item or places a schematic based on the type of construction recipe.
    /// </summary>
    private void ConstructItem(ConstructionPrototype prototype)
    {
        if (prototype.Type == ConstructionType.Item)
        {
            _construction.TryStartItemConstruction(prototype.ID);
            return;
        }

        _placementManager.BeginPlacing(new PlacementInformation
            {
                IsTile = false,
                PlacementOption = prototype.PlacementMode
            },
            new ConstructionPlacementHijack(_construction, prototype));

        // Should only close the menu if we're placing a construction hijack.
        // Theres not much point to closing it though. _menu!.Close();
    }
}
