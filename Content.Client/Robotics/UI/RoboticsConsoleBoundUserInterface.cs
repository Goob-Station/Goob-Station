// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Robotics;
using Robust.Client.UserInterface;

namespace Content.Client.Robotics.UI;

public sealed class RoboticsConsoleBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    public RoboticsConsoleWindow _window = default!;

    public RoboticsConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<RoboticsConsoleWindow>();
        _window.SetEntity(Owner);

        _window.OnDisablePressed += address =>
        {
            SendMessage(new RoboticsConsoleDisableMessage(address));
        };
        _window.OnDestroyPressed += address =>
        {
            SendMessage(new RoboticsConsoleDestroyMessage(address));
        };
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not RoboticsConsoleState cast)
            return;

        _window.UpdateState(cast);
    }
}