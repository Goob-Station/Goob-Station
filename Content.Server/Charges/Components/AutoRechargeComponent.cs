// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Charges.Systems;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.Charges.Components;

/// <summary>
/// Something with limited charges that can be recharged automatically.
/// Requires LimitedChargesComponent to function.
/// </summary>
// TODO: no reason this cant be predicted and server system deleted
[RegisterComponent, AutoGenerateComponentPause]
[Access(typeof(ChargesSystem))]
public sealed partial class AutoRechargeComponent : Component
{
    /// <summary>
    /// The time it takes to regain a single charge
    /// </summary>
    [DataField("rechargeDuration"), ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan RechargeDuration = TimeSpan.FromSeconds(90);

    /// <summary>
    /// The time when the next charge will be added
    /// </summary>
    [DataField("nextChargeTime", customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan NextChargeTime;
}