// SPDX-FileCopyrightText: 2025 FaDeOkno <logkedr18@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Server._Goobstation.Obsessed;

[RegisterComponent]
public sealed partial class ObsessionTargetComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public int Id = 0;
}
