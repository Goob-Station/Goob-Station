// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Client.Ghost;
using Content.Goobstation.Shared.Ghost.ObserverHud;
using Content.Shared.Ghost;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;

namespace Content.Goobstation.Client.Ghost.ObserverHud;

public sealed class GhostObserverHudUIController : UIController, IOnSystemChanged<GhostSystem>
{
    [Dependency] private readonly IEntityNetworkManager _net = default!;
    [UISystemDependency] private readonly GhostSystem? _ghost = default;

    private GhostObserverHudPopup? _popup;

    public void OnSystemLoaded(GhostSystem system)
    {
        SubscribeLocalEvent<ToggleGhostObserverHudActionEvent>(OnToggleAction);
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

    private void OnToggleAction(ToggleGhostObserverHudActionEvent args)
    {
        // Admin ghosts already have admin overlays, so only regular ghosts open this menu
        if (args.Handled ||
            _ghost?.Player is not { CanGhostInteract: false } ghost ||
            ghost.Owner != args.Performer)
        {
            return;
        }

        args.Handled = true;
        Popup.Open(UIBox2.FromDimensions(UIManager.MousePositionScaled.Position, Vector2.One));
    }

    private void OnVisualsChanged(GhostObserverHudVisuals visuals)
    {
        if (_ghost?.Player is not { CanGhostInteract: false })
            return;

        // Send the full checkbox selection to the server so it can apply the allowed HUD overlays
        _net.SendSystemNetworkMessage(new GhostObserverHudUpdateRequestEvent(visuals));
    }

    private void OnPlayerRemoved(GhostComponent _) => Reset();

    private void OnPlayerUpdated(GhostComponent component)
    {
        if (component.CanGhostInteract)
            Reset();
    }

    private void OnPlayerAttached(GhostComponent _) => Reset();

    private void OnPlayerDetached() => Reset();

    private GhostObserverHudPopup Popup
    {
        // Create the popup only when the observer opens the action menu for the first time
        get
        {
            if (_popup != null)
                return _popup;

            _popup = UIManager.CreatePopup<GhostObserverHudPopup>();
            _popup.VisualsChanged += OnVisualsChanged;
            return _popup;
        }
    }

    private void Reset()
    {
        _popup?.ClearSelection();
        _popup?.Close();
    }
}
