// SPDX-License-Identifier: AGPL-3.0-or-later


using Content.Client.UserInterface.Controls;
using Content.Shared.Silicons.StationAi;
using Robust.Client.UserInterface;

#region DOWNSTREAM-TPirates: borg wireless access
using Content.Shared.Access;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Robust.Client.Player;
using Robust.Shared.IoC;
using Robust.Shared.Prototypes;
#endregion

namespace Content.Client.Silicons.StationAi;

public sealed class StationAiBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private SimpleRadialMenu? _menu;

    #region DOWNSTREAM-TPirates: borg wireless access
    private IPlayerManager? _cachedPlayerManager;
    private IPlayerManager CachedPlayerManager => _cachedPlayerManager ??= IoCManager.Resolve<IPlayerManager>();
    #endregion

    protected override void Open()
    {
        base.Open();

        #region DOWNSTREAM-TPirates: borg wireless access
        // Syndicate borgs: open limited (two-action) radial and return.
        // Everyone else falls through to the normal AI radial below.
        if (ShowLimitedMenu())
            return;
        #endregion

        var ev = new GetStationAiRadialEvent();
        EntMan.EventBus.RaiseLocalEvent(Owner, ref ev);

        _menu = this.CreateWindow<SimpleRadialMenu>();
        _menu.Track(Owner);
        var buttonModels = ConvertToButtons(ev.Actions);
        _menu.SetButtons(buttonModels);

        _menu.Open();
    }

    private IEnumerable<RadialMenuActionOptionBase> ConvertToButtons(IReadOnlyList<StationAiRadial> actions)
    {
        var models = new RadialMenuActionOptionBase[actions.Count];
        for (int i = 0; i < actions.Count; i++)
        {
            var action = actions[i];
            models[i] = new RadialMenuActionOption<BaseStationAiAction>(HandleRadialMenuClick, action.Event)
            {
                IconSpecifier = RadialMenuIconSpecifier.With(action.Sprite),
                ToolTip = action.Tooltip
            };
        }

        return models;
    }

    private void HandleRadialMenuClick(BaseStationAiAction p)
    {
        SendPredictedMessage(new StationAiRadialMessage { Event = p });
    }

    #region DOWNSTREAM-TPirates: borg wireless access
    private bool ShowLimitedMenu()
    {
        var controlled = CachedPlayerManager.LocalPlayer?.ControlledEntity;

        if (controlled is null)
            return false;

        if (EntMan.System<AccessReaderSystem>().IsAllowed(controlled.Value, Owner))
            return false;

        var ev = new GetStationAiLimitedAirlockRadialEvent();
        EntMan.EventBus.RaiseLocalEvent(Owner, ref ev);

        _menu = this.CreateWindow<SimpleRadialMenu>();
        _menu.Track(Owner);
        var buttonModels = ConvertToButtons(ev.Actions);
        _menu.SetButtons(buttonModels);

        _menu.Open();
        return true;
    }
    #endregion
}
