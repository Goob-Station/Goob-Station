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
    private readonly RadialMenu _menu;

    public RuneDrawerBUI(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        _spriteSystem = _entManager.System<SpriteSystem>();
        _menu = new()
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
        _menu.OpenCenteredAt(_inputManager.MouseScreenPosition.Position / _displayManager.ScreenSize);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _menu.Close();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is RuneDrawerMenuState runeDrawerState)
            FillMenu(runeDrawerState.AvailalbeRunes);
    }

    private void FillMenu(List<ProtoId<RuneSelectorPrototype>>? runes = null)
    {
        if (runes is null)
            return;

        var container = new RadialContainer
        {
            Radius = 48f + 24f * MathF.Log(runes.Count)
        };

        _menu.AddChild(container);

        foreach (var runeSelector in runes)
        {
            if (!_protoManager.TryIndex(runeSelector, out var runeSelectorProto) ||
                !_protoManager.TryIndex(runeSelectorProto.Prototype, out var runeProto))
                continue;

            var itemSize = new Vector2(64f, 64f);
            var button = new RadialMenuTextureButton
            {
                ToolTip = Loc.GetString(runeProto.Name),
                StyleClasses = { "RadialMenuButton" },
                SetSize = itemSize
            };

            var runeIcon = _spriteSystem.Frame0(runeProto);
            var runeScale = itemSize / runeIcon.Size;

            var texture = new TextureRect
            {
                VerticalAlignment = Control.VAlignment.Center,
                HorizontalAlignment = Control.HAlignment.Center,
                Texture = _spriteSystem.Frame0(runeProto),
                TextureScale = runeScale
            };

            button.AddChild(texture);

            button.OnButtonUp += _ =>
            {
                SendMessage(new RuneDrawerSelectedMessage(runeSelector));
                Close();
            };

            container.AddChild(button);
        }
    }
}
