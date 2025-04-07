// SPDX-FileCopyrightText: 2024 Adeinitas <147965189+adeinitas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Danger Revolution! <142105406+DangerRevolution@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <plykiya@protonmail.com>
// SPDX-FileCopyrightText: 2024 Timemaster99 <57200767+Timemaster99@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2024 Verm <32827189+Vermidia@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.StepTrigger.Prototypes;
using Content.Shared.StepTrigger.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.StepTrigger.Components;
/// <summary>
///     Goobstation: This component marks an entity as being immune to all step triggers.
///     For example, a Harpy being so low density, that they don't set off landmines.
/// </summary>
/// <remarks>
///     This is the "Earliest Possible Exit" method, and therefore isn't possible to un-cancel.
///     It will prevent ALL step trigger events from firing. Therefore there may sometimes be unintended consequences to this.
///     Consider using a subscription to StepTriggerAttemptEvent if you wish to be more selective.
/// </remarks>
[RegisterComponent, NetworkedComponent]
[Access(typeof(StepTriggerSystem))]
public sealed partial class StepTriggerImmuneComponent : Component
{
    /// <summary>
    ///     WhiteList of immunity step triggers.
    /// </summary>
    [DataField]
    public StepTriggerGroup? Whitelist;
}