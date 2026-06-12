using Content.Client._pofitlo.CombatExtended.FightAction;
using Content.Client._pofitlo.CombatExtended.UserInterface.FightAction.UI;
using Content.Client._pofitlo.CombatExtended.UserInterface.FightAction.Widgets;
using Content.Client.Gameplay;
using Content.Client.UserInterface.Controls;
using Content.Shared._pofitlo.CombatExtended.FightAction;
using Content.Shared._pofitlo.CombatExtended.FightAction.Events;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Utility;
using System.Numerics;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;
using Content.Shared._pofitlo.CombatExtended.FightAction.Prototypes;


namespace Content.Client._pofitlo.CombatExtended.UserInterface.FightAction;

public sealed class FightActionUIController : UIController, IOnStateEntered<GameplayState>, IOnSystemChanged<FightActionSystem>
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IEntitySystemManager _entitySystem = default!;
    [Dependency] private readonly IEntityNetworkManager _net = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public event Action<FightActionComponent>? StrategyChange;

    private FightActionComponent? _fightActionComponent;
    private FightActionControl? FightActionControl => UIManager.GetActiveUIWidgetOrNull<FightActionControl>();
    private SpriteSystem? SpriteSystem => _entitySystem.GetEntitySystem<SpriteSystem>();
    private SimpleRadialMenu? _menu;
    private FightActionRadialMenu? _fightActionMenu;


    public void OnSystemLoaded(FightActionSystem system)
    {
        system.FightActionStartup += AddFightActionControl;
        //system.FightActionShutdown += RemoveFightActionControl;
        system.StrategyChange += WidgetIconChange;
    }

    public void OnSystemUnloaded(FightActionSystem system)
    {
        system.FightActionStartup -= AddFightActionControl;
        //system.FightActionShutdown -= RemoveFightActionControl;
        system.StrategyChange -= WidgetIconChange;
    }

    public void OnStateEntered(GameplayState state)
    {
        if (FightActionControl == null)
            return;

        FightActionControl.SetTargetDollVisible(_fightActionComponent != null);

        //if (_fightActionComponent != null)
            //FightActionControl.SetBodyPartsVisible(_fightActionComponent.Target);
    }

    public void AddFightActionControl(FightActionComponent component)
    {
        _fightActionComponent = component;

        if (FightActionControl != null)
        {
            FightActionControl.SetTargetDollVisible(_fightActionComponent != null);

            //if (_fightActionComponent != null)
                //FightActionControl.SetBodyPartsVisible(_fightActionComponent.Target);
        }

    }

    public void RemoveFightActionControl()
    {
        if (FightActionControl != null)
            FightActionControl.SetTargetDollVisible(false);

        _fightActionComponent = null;
    }

    public void WidgetIconChange(FightActionComponent component)
    {
        //FightActionControl.
    }

    public void ChangeWidgetIcon(Texture texture)
    {
        FightActionControl?.ChangeWidgetIcon(texture);
    }

    //public void CycleTarget(TargetBodyPart bodyPart)
    //{
    //    if (_playerManager.LocalEntity is not { } user
    //        || _entManager.GetComponent<FightActionComponent>(user) is not { } targetingComponent
    //        || FightActionControl == null)
    //        return;

    //    var player = _entManager.GetNetEntity(user);
    //    if (bodyPart != targetingComponent.Target)
    //    {
    //        var msg = new FightActionChangeEvent(player, bodyPart);
    //        _net.SendSystemNetworkMessage(msg);
    //        FightActionControl?.SetBodyPartsVisible(bodyPart);
    //    }
    //}

    //AttackStrategy fightAction, SpriteSpecifier icon, ProtoId<CombatAnimationPrototype> proto

    public void SetFightAction(FightActionPrototype fightActionPrototype)
    {
        if (_playerManager.LocalEntity is not { } user
            || !_entManager.TryGetComponent<FightActionComponent>(user, out var fightActionComp)
            || FightActionControl == null)
            return;

        ProtoId<CombatAnimationPrototype> animationProto = fightActionPrototype.AnimationPrototype;
        ProtoId<CombatAnimationPrototype> altAnimationProto = fightActionPrototype.AltAnimationPrototype;
        ProtoId<FightActionMeleeParametersPrototype> meleeParametersProto = fightActionPrototype.MeleeParametersPrototype;
        AttackStrategy fightAction = fightActionPrototype.SetAttackStrategy;
        SpriteSpecifier icon = fightActionPrototype.Icon;
        bool hasHigherPriorityThanWeapons = fightActionPrototype.HasHigherPriorityThanWeapons;

        var player = _entManager.GetNetEntity(user);
        if (fightAction != fightActionComp.Strategy)
        {
            var msg = new FightActionChangeEvent(player, fightAction, hasHigherPriorityThanWeapons, meleeParametersProto, animationProto, altAnimationProto);
            _net.SendSystemNetworkMessage(msg);
        }

        if (icon == null || SpriteSystem == null)
            return;
        var texture = SpriteSystem.Frame0(icon);
        ChangeWidgetIcon(texture);
    }

    public void ToggleMenu(Vector2 position)
    {
        if (_fightActionMenu == null)
        {

            _fightActionMenu = new FightActionRadialMenu();
            _fightActionMenu.RefreshUI();

            position -= _fightActionMenu.MinSize / 2;
            _fightActionMenu.OnClose += CloseMenu;
            _fightActionMenu.Open(position);
            //_menu.OpenCentered();
        }
        else
        {
            CloseMenu();
        }
    }

    public void CloseMenu()
    {
        if (_fightActionMenu == null)
            return;

        _fightActionMenu.OnClose -= CloseMenu;
        _fightActionMenu.Dispose(); // TODO избавиться от дизпоза
        _fightActionMenu = null;
    }

}
