// SPDX-FileCopyrightText: 2025 RichardBlonski <48651647+RichardBlonski@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Shitmed.Antags.Abductor;
using Robust.Client.UserInterface;

namespace Content.Goobstation.Client.Antagonists.Abductor.Abductee;

public sealed class AbducteeReturnPopupSystem : EntitySystem
{
    private AbducteeReturnPopup? _window;
    [Dependency] private readonly IUserInterfaceManager _uiManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AbductorVictimComponent, MoveEvent>(OnAbducteeReturned);
    }

    private void OnAbducteeReturned(EntityUid uid, AbductorVictimComponent component, MoveEvent args)
    {
        // Only Open the popup if the abductee has been returned to their original position
        // Aka when you finished the Experiment as the Abductor
        if (component.Position.HasValue
            && args.NewPosition.Equals(component.Position.Value))
            OpenAbducteePopup();
    }

    public void OpenAbducteePopup() // Change to private
    {

        if (_window != null)
        {
            _window.MoveToFront();
        return;
        }


        _window = new AbducteeReturnPopup();
        _window.OnClose += () => _window = null;

        _uiManager.WindowRoot.AddChild(_window);
        _window.OpenCentered();
    }
}
