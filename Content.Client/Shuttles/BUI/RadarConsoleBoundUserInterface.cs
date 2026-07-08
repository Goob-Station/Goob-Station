// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Shuttles.BUIStates;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using RadarConsoleWindow = Content.Client.Shuttles.UI.RadarConsoleWindow;

namespace Content.Client.Shuttles.BUI;

[UsedImplicitly]
public sealed class RadarConsoleBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private RadarConsoleWindow? _window;

    public RadarConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<RadarConsoleWindow>();
        _window?.SetConsole(Owner); // Frontier
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is not NavBoundUserInterfaceState cState)
            return;

        _window?.UpdateState(cState.State);
    }
}
