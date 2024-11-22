using System.Numerics;
using Content.Client.Popups;
using Content.Client.UserInterface.Controls;
using Content.Shared.FixedPoint;
using Content.Shared.WhiteDream.BloodCult.UI;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;

namespace Content.Client.WhiteDream.BloodCult.UI;

[UsedImplicitly]
public sealed class BloodRitesUi : BoundUserInterface
{
    [Dependency] private readonly IClyde _displayManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;

    private readonly PopupSystem _popup;
    private readonly SpriteSystem _sprite;
    private readonly Vector2 _itemSize = Vector2.One * 64;

    private RadialMenu? _menu;
    private FixedPoint2 _storedBlood;

    public BloodRitesUi(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        _sprite = _entManager.System<SpriteSystem>();
        _popup = _entManager.System<PopupSystem>();
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

        _menu.OpenCenteredAt(_inputManager.MouseScreenPosition.Position / _displayManager.ScreenSize);
        _menu.OnClose += Close;
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not BloodRitesUiState ritesState)
            return;

        CreateMenu(ritesState.Crafts);
        _storedBlood = ritesState.StoredBlood;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing && _menu is not null)
            _menu.Dispose();
    }

    private void CreateMenu(Dictionary<EntProtoId, float> crafts)
    {
        if (_menu is null)
            return;

        var container = new RadialContainer
        {
            Name = "Blood Rites",
            Radius = 64f + 32f * MathF.Log(crafts.Count),
        };

        _menu.AddChild(container);

        foreach (var (protoId, cost) in crafts)
        {
            if (!_protoManager.TryIndex(protoId, out var proto))
                return;

            var name = $"{cost}: {proto.Name}";
            var button = CreateButton(name, _sprite.Frame0(proto));
            button.OnButtonUp += _ =>
            {
                TryCraft(protoId, cost);
            };

            container.AddChild(button);
        }
    }

    private RadialMenuTextureButton CreateButton(string name, Texture icon)
    {
        var button = new RadialMenuTextureButton
        {
            ToolTip = Loc.GetString(name),
            StyleClasses = { "RadialMenuButton" },
            SetSize = _itemSize
        };

        var iconScale = _itemSize / icon.Size;
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

    private void TryCraft(EntProtoId protId, FixedPoint2 cost)
    {
        if (cost > _storedBlood)
        {
            _popup.PopupEntity(Loc.GetString("blood-rites-not-enough-blood"), Owner);
            return;
        }

        _storedBlood -= cost;
        var msg = new BloodRitesMessage(protId);
        SendPredictedMessage(msg);
    }
}
