using System.Linq;
using System.Numerics;
using Content.Client.Construction;
using Content.Client.UserInterface.Controls;
using Content.Shared._White.RadialSelector;
using Content.Shared.Construction.Prototypes;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;

// ReSharper disable InconsistentNaming

namespace Content.Client._White.RadialSelector;

public abstract class BasedRadialSelectorMenuBUI : BoundUserInterface
{
    [Dependency] protected readonly IPrototypeManager ProtoManager = default!;
    [Dependency] protected readonly IResourceCache Resources = default!;

    protected readonly ConstructionSystem _constructionSystem;
    protected readonly SpriteSystem _spriteSystem;

    // Used to clearing on state changing
    private readonly HashSet<RadialContainer> _cachedContainers = new();

    private readonly Vector2 ItemSize = Vector2.One * 64;

    protected BasedRadialSelectorMenuBUI(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        _constructionSystem = EntMan.System<ConstructionSystem>();
        _spriteSystem = EntMan.System<SpriteSystem>();
    }

    protected void CreateMenu(List<RadialSelectorEntry> entries, RadialMenu menu, string parentCategory = "")
    {
        var container = new RadialContainer
        {
            Name = !string.IsNullOrEmpty(parentCategory) ? parentCategory : "Main",
            InitialRadius = 48f + 24f * MathF.Log(entries.Count),
        };

        menu.AddChild(container);
        _cachedContainers.Add(container);

        foreach (var entry in entries)
        {
            if (entry.Category != null)
            {
                var button = CreateButton(entry.Category.Name, _spriteSystem.Frame0(entry.Category.Icon));
                button.TargetLayer = container;
                CreateMenu(entry.Category.Entries, menu, entry.Category.Name);
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
        if (ProtoManager.TryIndex(proto, out var prototype))
            return prototype.Name;

        if (ProtoManager.TryIndex(proto, out ConstructionPrototype? constructionPrototype))
            return constructionPrototype.Name ?? proto;

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

        if (ProtoManager.TryIndex(entry.Prototype!, out var prototype))
        {
            result.AddRange(SpriteComponent.GetPrototypeTextures(prototype, Resources).Select(o => o.Default));
            return result;
        }

        if (ProtoManager.TryIndex(entry.Prototype!, out ConstructionPrototype? constructionProto)
            && _constructionSystem.TryGetRecipePrototype(constructionProto.ID, out var targetProtoId)
            && ProtoManager.TryIndex(targetProtoId, out EntityPrototype? proto))
        {
            result.AddRange(SpriteComponent.GetPrototypeTextures(proto, Resources).Select(o => o.Default));
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

    protected void ClearExistingContainers(RadialMenu menu)
    {
        foreach (var container in _cachedContainers)
            menu.RemoveChild(container);

        _cachedContainers.Clear();
    }
}
