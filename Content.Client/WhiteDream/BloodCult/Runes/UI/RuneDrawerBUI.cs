using System.Linq;
using System.Numerics;
using Content.Client.UserInterface.Controls;
using Content.Shared.WhiteDream.BloodCult.Runes;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;

// ReSharper disable InconsistentNaming

namespace Content.Client.WhiteDream.BloodCult.Runes.UI;

[UsedImplicitly]
public sealed class RuneDrawerBUI : BoundUserInterface
{
    [Dependency] private readonly EntityManager _entManager = default!;
    [Dependency] private readonly IClyde _displayManager = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;

    private readonly SpriteSystem _spriteSystem;

    private RadialMenu? _menu;

    public RuneDrawerBUI(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
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

        if (!_entManager.HasComponent<RuneDrawerComponent>(Owner))
            return menu;

        var runeSelectorArray = _protoManager.EnumeratePrototypes<RuneSelectorPrototype>().OrderBy(r => r.ID).ToArray();

        var mainContainer = new RadialContainer
        {
            Radius = 36f / (runeSelectorArray.Length == 1
                ? 1
                : MathF.Sin(MathF.PI / runeSelectorArray.Length))
        };

        foreach (var runeSelector in runeSelectorArray)
        {
            if (!_protoManager.TryIndex(runeSelector.Prototype, out var proto))
                continue;

            var itemSize = new Vector2(64f, 64f);
            var button = new RadialMenuTextureButton
            {
                ToolTip = Loc.GetString(proto.Name),
                StyleClasses = { "RadialMenuButton" },
                SetSize = itemSize
            };

            var runeIcon = _spriteSystem.Frame0(proto);
            var runeScale = itemSize / runeIcon.Size;

            var texture = new TextureRect
            {
                VerticalAlignment = Control.VAlignment.Center,
                HorizontalAlignment = Control.HAlignment.Center,
                Texture = _spriteSystem.Frame0(proto),
                TextureScale = runeScale
            };

            button.AddChild(texture);

            button.OnButtonUp += _ =>
            {
                SendMessage(new RuneDrawerSelectedMessage(runeSelector));
                Close();
            };

            mainContainer.AddChild(button);
        }

        menu.AddChild(mainContainer);
        return menu;
    }
}
