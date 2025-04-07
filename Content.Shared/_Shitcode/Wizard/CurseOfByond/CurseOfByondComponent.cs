// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
namespace Content.Shared._Goobstation.Wizard.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class CurseOfByondComponent : Component
{
    [DataField]
    public string CurseOfByondAlertKey = "CurseOfByond";
}