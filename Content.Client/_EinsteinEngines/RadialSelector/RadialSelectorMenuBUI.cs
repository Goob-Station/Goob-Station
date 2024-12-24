using System.Linq;
using System.Numerics;
using Content.Client.UserInterface.Controls;
using Content.Shared.Construction.Prototypes;
using Content.Shared.RadialSelector;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;

// ReSharper disable InconsistentNaming

namespace Content.Client.RadialSelector;

[UsedImplicitly]
public sealed class RadialSelectorMenuBUI : BoundUserInterface
{
    [Dependency] private readonly IClyde _displayManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly IResourceCache _resources = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly IEntityManager _entManager = default!;

    private readonly SpriteSystem _spriteSystem;

    private readonly RadialMenu _menu;

    // Used to clearing on state changing
    private readonly HashSet<RadialContainer> _cachedContainers = new();

    private bool _openCentered;
    private readonly Vector2 ItemSize = Vector2.One * 64;

    public RadialSelectorMenuBUI(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        _spriteSystem = _entManager.System<SpriteSystem>();
        _menu = new RadialMenu
        {
            HorizontalExpand = true,
            VerticalExpand = true,
            BackButtonStyleClass = "RadialMenuBackButton",
            CloseButtonStyleClass = "RadialMenuCloseButton"
        };
    }

    protected override void Open()
    {
        _menu.OnClose += Close;

        if (_openCentered)
            _menu.OpenCentered();
        else
            _menu.OpenCenteredAt(_inputManager.MouseScreenPosition.Position / _displayManager.ScreenSize);
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not RadialSelectorState radialSelectorState)
            return;

        ClearExistingContainers();
        CreateMenu(radialSelectorState.Entries);
        _openCentered = radialSelectorState.OpenCentered;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _menu.Dispose();
    }

    private void CreateMenu(List<RadialSelectorEntry> entries, string parentCategory = "")
    {
        var container = new RadialContainer
        {
            Name = !string.IsNullOrEmpty(parentCategory) ? parentCategory : "Main",
            Radius = 48f + 24f * MathF.Log(entries.Count),
        };

        _menu.AddChild(container);
        _cachedContainers.Add(container);

        foreach (var entry in entries)
        {
            if (entry.Category != null)
            {
                var button = CreateButton(entry.Category.Name, _spriteSystem.Frame0(entry.Category.Icon));
                button.TargetLayer = entry.Category.Name;
                CreateMenu(entry.Category.Entries, entry.Category.Name);
                container.AddChild(button);
            }
            else if (entry.Prototype != null)
            {
                var name = GetName(entry.Prototype);
                var icon = GetTextures(entry);
                var button = CreateButton(name, icon);
                button.OnButtonUp += _ =>
                {
                    var msg = new RadialSelectorSelectedMessage(entry.Prototype);
                    SendPredictedMessage(msg);
                };

                container.AddChild(button);
            }
        }
    }

    private string GetName(string proto)
    {
        if (_protoManager.TryIndex(proto, out var prototype))
            return prototype.Name;

        if (_protoManager.TryIndex(proto, out ConstructionPrototype? constructionPrototype))
            return constructionPrototype.Name;

        return proto;
    }

    private List<Texture> GetTextures(RadialSelectorEntry entry)
    {
        var result = new List<Texture>();
        if (entry.Icon is not null)
        {
            result.Add(_spriteSystem.Frame0(entry.Icon));
            return result;
        }

        if (_protoManager.TryIndex(entry.Prototype!, out var prototype))
        {
            result.AddRange(SpriteComponent.GetPrototypeTextures(prototype, _resources).Select(o => o.Default));
            return result;
        }

        if (_protoManager.TryIndex(entry.Prototype!, out ConstructionPrototype? constructionProto))
        {
            result.Add(_spriteSystem.Frame0(constructionProto.Icon));
            return result;
        }

        // No icons provided and no icons found in prototypes. There's nothing we can do.
        return result;
    }

    private RadialMenuTextureButton CreateButton(string name, Texture icon)
    {
        var button = new RadialMenuTextureButton
        {
            ToolTip = Loc.GetString(name),
            StyleClasses = { "RadialMenuButton" },
            SetSize = ItemSize
        };

        var iconScale = ItemSize / icon.Size;
        var texture = new TextureRect
        {
            VerticalAlignment = Control.VAlignment.Center,
            HorizontalAlignment = Control.HAlignment.Center,
            Texture = icon,
            TextureScale = iconScale
        };

        button.AddChild(texture);
        return button;
    }

    private RadialMenuTextureButton CreateButton(string name, List<Texture> icons)
    {
        var button = new RadialMenuTextureButton
        {
            ToolTip = Loc.GetString(name),
            StyleClasses = { "RadialMenuButton" },
            SetSize = ItemSize
        };

        var iconScale = ItemSize / icons[0].Size;
        var texture = new LayeredTextureRect
        {
            VerticalAlignment = Control.VAlignment.Center,
            HorizontalAlignment = Control.HAlignment.Center,
            Textures = icons,
            TextureScale = iconScale
        };

        button.AddChild(texture);
        return button;
    }

    private void ClearExistingContainers()
    {
        foreach (var container in _cachedContainers)
            _menu.RemoveChild(container);

        _cachedContainers.Clear();
    }
}
