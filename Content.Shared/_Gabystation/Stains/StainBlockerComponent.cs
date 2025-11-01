// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Will-Oliver-Br <164823659+Will-Oliver-Br@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Inventory;
using Robust.Shared.GameStates;

namespace Content.Shared.Stains.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class StainBlockerComponent : Component
{
    [DataField("slots", required: true)]
    public SlotFlags Slots;
}
