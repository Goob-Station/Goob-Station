// SPDX-FileCopyrightText: 2025 FaDeOkno <logkedr18@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Obsessed;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ObsessionTargetComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public int Id = 0;
}
