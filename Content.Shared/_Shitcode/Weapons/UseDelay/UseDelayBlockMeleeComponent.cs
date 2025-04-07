// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Weapons.UseDelay;

[RegisterComponent, NetworkedComponent]
public sealed partial class UseDelayBlockMeleeComponent : Component
{
    [DataField]
    public List<string> Delays = new(){"default"};
}