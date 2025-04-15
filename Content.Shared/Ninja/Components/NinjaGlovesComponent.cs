// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <deltanedas@laptop>
// SPDX-FileCopyrightText: 2023 deltanedas <user@zenith>
// SPDX-FileCopyrightText: 2023 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tayrtahn <tayrtahn@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Ninja.Systems;
using Content.Shared.Objectives.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Ninja.Components;

/// <summary>
/// Component for toggling glove powers.
/// </summary>
/// <remarks>
/// Requires <c>ItemToggleComponent</c>.
/// </remarks>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedNinjaGlovesSystem))]
public sealed partial class NinjaGlovesComponent : Component
{
    /// <summary>
    /// Entity of the ninja using these gloves, usually means enabled
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? User;

    /// <summary>
    /// Abilities to give to the user when enabled.
    /// </summary>
    [DataField(required: true)]
    public List<NinjaGloveAbility> Abilities = new();
}

/// <summary>
/// An ability that adds components to the user when the gloves are enabled.
/// </summary>
[DataRecord]
public partial record struct NinjaGloveAbility()
{
    /// <summary>
    /// If not null, checks if an objective with this prototype has been completed.
    /// If it has, the ability components are skipped to prevent doing the objective twice.
    /// The objective must have <c>CodeConditionComponent</c> to be checked.
    /// </summary>
    [DataField]
    public EntProtoId<ObjectiveComponent>? Objective;

    /// <summary>
    /// Components to add and remove.
    /// </summary>
    [DataField(required: true)]
    public ComponentRegistry Components = new();
}