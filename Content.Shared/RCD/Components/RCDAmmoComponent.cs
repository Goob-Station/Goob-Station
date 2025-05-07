// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2024 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2024 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.FixedPoint;
using Content.Shared.RCD.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.RCD.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(RCDAmmoSystem))]
public sealed partial class RCDAmmoComponent : Component
{
    /// <summary>
    /// How many charges are contained in this ammo cartridge.
    /// Can be partially transferred into an RCD, until it is empty then it gets deleted.
    /// </summary>
    [DataField("charges"), ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public FixedPoint2 Charges = 30;
}