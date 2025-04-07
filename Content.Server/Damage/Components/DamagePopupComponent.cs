// SPDX-FileCopyrightText: 2022 ZeroDayDaemon <60460608+ZeroDayDaemon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Dae <60460608+ZeroDayDaemon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Damage.Systems;

namespace Content.Server.Damage.Components;

[RegisterComponent, Access(typeof(DamagePopupSystem))]
public sealed partial class DamagePopupComponent : Component
{
    /// <summary>
    /// Bool that will be used to determine if the popup type can be changed with a left click.
    /// </summary>
    [DataField("allowTypeChange")] [ViewVariables(VVAccess.ReadWrite)]
    public bool AllowTypeChange = false;
    /// <summary>
    /// Enum that will be used to determine the type of damage popup displayed.
    /// </summary>
    [DataField("damagePopupType")] [ViewVariables(VVAccess.ReadWrite)]
    public DamagePopupType Type = DamagePopupType.Combined;
}
public enum DamagePopupType
{
    Combined,
    Total,
    Delta,
    Hit,
};