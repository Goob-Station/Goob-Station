// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Pirate.BladeServer;
using Content.Shared.Construction;
using Content.Shared.Construction.Components;
using JetBrains.Annotations;
using Robust.Shared.Containers;

namespace Content.Shared.Construction.NodeEntities;

/// <summary>
///     Works for both <see cref="ComputerBoardComponent"/> and <see cref="MachineBoardComponent"/>
///     because duplicating code just for this is really stinky.
/// </summary>
[UsedImplicitly]
[DataDefinition]
public sealed partial class BoardNodeEntity : IGraphNodeEntity
{
    [DataField]
    public string Container { get; private set; } = string.Empty;

    public string? GetId(EntityUid? uid, EntityUid? userUid, GraphNodeEntityArgs args)
    {
        if (uid == null)
            return null;

        var containerSystem = args.EntityManager.EntitySysManager.GetEntitySystem<SharedContainerSystem>();

        if (!containerSystem.TryGetContainer(uid.Value, Container, out var container)
            || container.ContainedEntities.Count == 0)
            return null;

        var board = container.ContainedEntities[0];

        // Pirate: blade server construction uses machine boards with an extra blade target.
        if (args.EntityManager.HasComponent<BladeServerFrameComponent>(uid) &&
            args.EntityManager.TryGetComponent<BladeServerBoardComponent>(board, out var bladeServer))
            return bladeServer.Prototype;

        // There should not be a case where more than one of these components exist on the same entity
        if (args.EntityManager.TryGetComponent(board, out MachineBoardComponent? machine))
            return machine.Prototype;

        if (args.EntityManager.TryGetComponent(board, out ComputerBoardComponent? computer))
            return computer.Prototype;

        if (args.EntityManager.TryGetComponent(board, out ElectronicsBoardComponent? electronics))
            return electronics.Prototype;

        return null;
    }
}
