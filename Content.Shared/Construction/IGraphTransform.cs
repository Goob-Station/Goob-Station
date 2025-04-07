// SPDX-FileCopyrightText: 2023 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

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