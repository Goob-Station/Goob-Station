// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Content.Shared.Whitelist; // PIRATE imp

namespace Content.Shared.Chemistry.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ReagentTankComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 TransferAmount { get; set; } = FixedPoint2.New(10);

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public ReagentTankType TankType { get; set; } = ReagentTankType.Unspecified;

    // PIRATE V
    // imp start
    // white list and black list function on the tanks lets them ensure that only the intended tools/equipment can refuel at them.
    [DataField]
    public EntityWhitelist? FuelWhitelist;

    [DataField]
    public EntityWhitelist? FuelBlacklist;
    // imp end
    // PIRATE ^
}

[Serializable, NetSerializable]
public enum ReagentTankType : byte
{
    Unspecified,
    Fuel
}
