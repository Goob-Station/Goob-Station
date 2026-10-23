// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Client.Ghost;
using Content.Goobstation.Shared.Observer.Hud;
using Content.Shared.Ghost;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;

namespace Content.Goobstation.Client.Observer.Hud;

public sealed class ObserverHudUIController : UIController, IOnSystemChanged<GhostSystem>
{
    [Dependency] private readonly IEntityNetworkManager _net = default!;
    [UISystemDependency] private readonly GhostSystem? _ghost = default;

    private ObserverHudPopup? _popup;

    public void OnSystemLoaded(GhostSystem system)
    {
        SubscribeLocalEvent<ToggleObserverHudActionEvent>(OnToggleAction);
        system.PlayerRemoved += OnPlayerRemoved;
        system.PlayerUpdated += OnPlayerUpdated;
        system.PlayerAttached += OnPlayerAttached;
        system.PlayerDetached += OnPlayerDetached;
    }

    public void OnSystemUnloaded(GhostSystem system)
    {
        Reset();
        system.PlayerRemoved -= OnPlayerRemoved;
        system.PlayerUpdated -= OnPlayerUpdated;
        system.PlayerAttached -= OnPlayerAttached;
        system.PlayerDetached -= OnPlayerDetached;
    }

    private void OnToggleAction(ToggleObserverHudActionEvent args)
    {
        var ghost = _ghost?.Player;

        if (args.Handled ||
            ghost == null ||
            !ghost.CanUseObserverHud ||
            ghost.Owner != args.Performer)
        {
            return;
        }

        args.Handled = true;
        GetPopup().Open(UIBox2.FromDimensions(UIManager.MousePositionScaled.Position, Vector2.One));
    }

    private void OnHudOptionsChanged(
        bool showJobMindshield,
        bool showHealth,
        bool showCriminalRecords)
    {
        var ghost = _ghost?.Player;

        if (ghost == null || !ghost.CanUseObserverHud)
            return;

        _net.SendSystemNetworkMessage(new ObserverHudUpdateRequestEvent(
            showJobMindshield, showHealth, showCriminalRecords));
    }

    private void OnPlayerRemoved(GhostComponent _)
    {
        Reset();
    }

    private void OnPlayerUpdated(GhostComponent component)
    {
        if (!component.CanUseObserverHud)
            Reset();
    }

    private void OnPlayerAttached(GhostComponent _)
    {
        Reset();
    }

    private void OnPlayerDetached()
    {
        Reset();
    }

    private ObserverHudPopup GetPopup()
    {
        if (_popup != null)
            return _popup;

        _popup = UIManager.CreatePopup<ObserverHudPopup>();
        _popup.HudOptionsChanged += OnHudOptionsChanged;

        return _popup;
    }

    private void Reset()
    {
        _popup?.ClearSelection();
        _popup?.Close();
    }
}
