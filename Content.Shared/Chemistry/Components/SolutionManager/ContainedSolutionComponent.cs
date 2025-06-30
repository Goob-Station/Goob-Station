// SPDX-FileCopyrightText: 2023 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2023 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry.EntitySystems;
using Robust.Shared.GameStates;

namespace Content.Shared.Chemistry.Components.SolutionManager;

/// <summary>
/// Component used to relate a solution to its container.
/// </summary>
/// <remarks>
/// When containers are finally ECS'd have this attach to the container entity.
/// The <see cref="Solution.MaxVolume"/> field should then be extracted out into this component.
/// Solution entities would just become an apporpriately composed entity hanging out in the container.
/// Will probably require entities in components being given a relation to associate themselves with their container.
/// </remarks>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedSolutionContainerSystem))]
public sealed partial class ContainedSolutionComponent : Component
{
    /// <summary>
    /// The entity that the solution is contained in.
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public EntityUid Container;

    /// <summary>
    /// The name/key of the container the solution is located in.
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public string ContainerName = default!;
}