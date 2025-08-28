using System.Numerics;
using Content.Client._pofitlo.CombatExtended.FightAction;
using Content.Client.Gameplay;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.Player;
using Content.Client._pofitlo.CombatExtended.UserInterface.FightAction.Widgets;
using Content.Shared._pofitlo.CombatExtended.FightAction;
using Content.Client.UserInterface.Controls;
using Content.Client._pofitlo.CombatExtended.UserInterface.FightAction.UI;
using Content.Shared._pofitlo.CombatExtended.FightAction.Events;
using Robust.Shared.Utility;
using Robust.Client.Graphics;


namespace Content.Client._pofitlo.CombatExtended.UserInterface.FightAction;

public sealed class FightActionUIController : UIController, IOnStateEntered<GameplayState>, IOnSystemChanged<FightActionSystem>
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IEntityNetworkManager _net = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    private FightActionComponent? _fightActionComponent;
    private FightActionControl? FightActionControl => UIManager.GetActiveUIWidgetOrNull<FightActionControl>();
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

    public void SetFightAction(AttackStrategy fightAction, SpriteSpecifier icon)
    {
        if (_playerManager.LocalEntity is not { } user
            || _entManager.GetComponent<FightActionComponent>(user) is not { } fightActionComp
            || FightActionControl == null)
            return;

        var player = _entManager.GetNetEntity(user);
        if (fightAction != fightActionComp.Strategy)
        {
            var msg = new FightActionChangeEvent(player, fightAction, icon);
            _net.SendSystemNetworkMessage(msg);
        }
    }

    public void ToggleMenu(Vector2 position)
    {
        if (_fightActionMenu == null)
        {

            _fightActionMenu = new FightActionRadialMenu();
            _fightActionMenu.RefreshUI();

            position -= _fightActionMenu.MinSize / 2;
            _fightActionMenu.Open(position);
            //_menu.OpenCentered();
        }
        else
        {
            _fightActionMenu.Dispose();
            _fightActionMenu = null;
        }
    }

}
