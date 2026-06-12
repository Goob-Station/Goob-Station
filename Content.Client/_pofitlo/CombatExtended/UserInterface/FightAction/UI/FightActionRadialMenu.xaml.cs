using Content.Client.UserInterface.Controls;
using Content.Shared._pofitlo.CombatExtended.FightAction;
using Content.Shared._pofitlo.CombatExtended.FightAction.Prototypes;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using System.Numerics;

namespace Content.Client._pofitlo.CombatExtended.UserInterface.FightAction.UI;

public sealed partial class FightActionRadialMenu : RadialMenu
{
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly IEntitySystemManager _entitySystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;
    private readonly SpriteSystem _spriteSystem;
    private readonly FightActionUIController _controller;
    public FightActionRadialMenu()
    {
        IoCManager.InjectDependencies(this);
        RobustXamlLoader.Load(this);
        ContextualButton.Visible = false;
        _spriteSystem = _entitySystem.GetEntitySystem<SpriteSystem>();
        _controller = UserInterfaceManager.GetUIController<FightActionUIController>();
    }

    public void RefreshUI()
    {
        var main = FindControl<RadialContainer>("Main"); // TODO сделать бордер для меню
        if (main == null)
            return;

        main.AngularRange = new Vector2(0f, MathF.PI / 4f);
        main.InitialRadius = 65f; // TODO сделать в зависимости от скейла экрана
        main.InnerRadiusMultiplier = 0.7f;
        main.OuterRadiusMultiplier = 1.3f;

        var player = _playerManager.LocalEntity;

        if (!_entityManager.TryGetComponent<FightActionComponent>(player, out var fightActionComp))
            return;

        foreach (var fightAction in fightActionComp.AvailableActions)
        {
            if (!_prototypeManager.TryIndex(fightAction, out var fightActionPrototype))
                continue;

            var button = new FightActionMenuButton()
            {
                SetSize = new Vector2(32, 32),
                ToolTip = Loc.GetString(fightActionPrototype.LocName),
                ProtoId = fightActionPrototype.ID
            };

            button.OnPressed += _ => _controller.SetFightAction(fightActionPrototype);

            var texture = new TextureRect
            {
                VerticalAlignment = VAlignment.Center,
                HorizontalAlignment = HAlignment.Center,
                Texture = _spriteSystem.Frame0(fightActionPrototype.Icon),
                //TextureScale = new Vector2(2f, 2f)
            };

            button.AddChild(texture);
            main.AddChild(button);
        }
    }

    public sealed class FightActionMenuButton : RadialMenuTextureButtonWithSector
    {
        public FightActionMenuButton()
        {
            IRadialMenuItemWithSector sector = this;
            DrawBorder = true;
        }
        public ProtoId<FightActionPrototype> ProtoId { get; set; }
    }
}
