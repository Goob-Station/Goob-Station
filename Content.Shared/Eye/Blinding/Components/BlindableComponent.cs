// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nairod <110078045+Nairodian@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Moomoobeef <62638182+Moomoobeef@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deathride58 <deathride58@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Eye.Blinding.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Eye.Blinding.Components;

[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(BlindableSystem))]
public sealed partial class BlindableComponent : Component
{
    /// <summary>
    /// How many seconds will be subtracted from each attempt to add blindness to us?
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("isBlind"), AutoNetworkedField]
    public bool IsBlind;

    /// <summary>
    /// Eye damage due to things like staring directly at welders. Causes blurry vision or outright
    /// blindness if greater than or equal to <see cref="MaxDamage"/>.
    /// </summary>
    /// <remarks>
    /// Should eventually be replaced with a proper eye health system when we have bobby.
    /// </remarks>
    [ViewVariables(VVAccess.ReadWrite), DataField("EyeDamage"), AutoNetworkedField]
    public int EyeDamage = 0;

    [ViewVariables(VVAccess.ReadOnly), DataField]
    public int MaxDamage = 9;

    [ViewVariables(VVAccess.ReadOnly), DataField]
    public int MinDamage = 0;

    /// <description>
    /// Used to ensure that this doesn't break with sandbox or admin tools.
    /// This is not "enabled/disabled".
    /// </description>
    [Access(Other = AccessPermissions.ReadWriteExecute)]
    public bool LightSetup = false;

    /// <description>
    /// Gives an extra frame of blindness to reenable light manager during
    /// </description>
    [Access(Other = AccessPermissions.ReadWriteExecute)]
    public bool GraceFrame = false;
}