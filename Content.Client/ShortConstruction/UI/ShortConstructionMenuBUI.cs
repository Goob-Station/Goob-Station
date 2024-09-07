using System.Numerics;
using Content.Client.Construction;
using Content.Client.UserInterface.Controls;
using Content.Shared.Construction.Prototypes;
using Content.Shared.ShortConstruction;
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

namespace Content.Client.ShortConstruction.UI;

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
        _menu = FormMenu();
        _menu.OnClose += Close;
        _menu.OpenCenteredAt(_inputManager.MouseScreenPosition.Position / _displayManager.ScreenSize);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _menu?.Dispose();
    }

    private RadialMenu FormMenu()
    {
        var menu = new RadialMenu
        {
            HorizontalExpand = true,
            VerticalExpand = true,
            BackButtonStyleClass = "RadialMenuBackButton",
            CloseButtonStyleClass = "RadialMenuCloseButton"
        };

        if (!_entManager.TryGetComponent<ShortConstructionComponent>(Owner, out var crafting))
            return menu;

        var mainContainer = new RadialContainer
        {
            Radius = 36f / MathF.Sin(MathF.PI / crafting.Prototypes.Count)
        };

        foreach (var protoId in crafting.Prototypes)
        {
            if (!_protoManager.TryIndex(protoId, out var proto))
                continue;

            var button = new RadialMenuTextureButton
            {
                ToolTip = Loc.GetString(proto.Name),
                StyleClasses = { "RadialMenuButton" },
                SetSize = new Vector2(48f, 48f)
            };

            var texture = new TextureRect
            {
                VerticalAlignment = Control.VAlignment.Center,
                HorizontalAlignment = Control.HAlignment.Center,
                Texture = _spriteSystem.Frame0(proto.Icon),
                TextureScale = new Vector2(1.5f, 1.5f)
            };

            button.AddChild(texture);

            button.OnButtonUp += _ =>
            {
                ConstructItem(proto);
            };

            mainContainer.AddChild(button);
        }

        menu.AddChild(mainContainer);
        return menu;
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
        }, new ConstructionPlacementHijack(_construction, prototype));

        // Should only close the menu if we're placing a construction hijack.
        _menu!.Close();
    }
}
