// SPDX-FileCopyrightText: 2021 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2023 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2023 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Chemistry.EntitySystems;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;

namespace Content.Shared.Chemistry.Components.SolutionManager;

/// <summary>
/// <para>A map of the solution entities contained within this entity.</para>
/// <para>Every solution entity this maps should have a <see cref="SolutionComponent"/> to track its state and a <see cref="ContainedSolutionComponent"/> to track its container.</para>
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedSolutionContainerSystem))]
public sealed partial class SolutionContainerManagerComponent : Component
{
    /// <summary>
    /// The default amount of space that will be allocated for solutions in solution containers.
    /// Most solution containers will only contain 1-2 solutions.
    /// </summary>
    public const int DefaultCapacity = 2;

    /// <summary>
    /// The names of each solution container attached to this entity.
    /// Actually accessing them must be done via <see cref="ContainerManagerComponent"/>.
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<string> Containers = new(DefaultCapacity);

    /// <summary>
    /// The set of solutions to load onto this entity during mapinit.
    /// </summary>
    /// <remarks>
    /// Should be null after mapinit.
    /// </remarks>
    [DataField, AutoNetworkedField]
    public Dictionary<string, Solution>? Solutions = null;
}