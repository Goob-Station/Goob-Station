// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Shared.Construction;

public interface IGraphTransform
{
    public void Transform(EntityUid oldUid, EntityUid newUid, EntityUid? userUid, GraphTransformArgs args);
}

public readonly struct GraphTransformArgs
{
    public readonly IEntityManager EntityManager;

    public GraphTransformArgs(IEntityManager entityManager)
    {
        EntityManager = entityManager;
    }
}
