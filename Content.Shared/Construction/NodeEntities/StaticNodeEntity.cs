// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Construction.NodeEntities;

[UsedImplicitly]
[DataDefinition]
public sealed partial class StaticNodeEntity : IGraphNodeEntity
{
    [DataField("id", customTypeSerializer:typeof(PrototypeIdSerializer<EntityPrototype>))]
    [ViewVariables(VVAccess.ReadWrite)]
    public string? Id { get; private set; }

    public StaticNodeEntity()
    {
    }

    public StaticNodeEntity(string id)
    {
        Id = id;
    }

    public string? GetId(EntityUid? uid, EntityUid? userUid, GraphNodeEntityArgs args)
    {
        return Id;
    }
}