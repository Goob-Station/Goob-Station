// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;


namespace Content.Shared.Heretic.Prototypes;

[DataDefinition]
public sealed partial class EventHereticAscension : EntityEventArgs;

[DataDefinition]
public sealed partial class EventHereticRerollTargets : EntityEventArgs;

[DataDefinition]
public sealed partial class EventHereticUpdateTargets : EntityEventArgs;

[DataDefinition]
public sealed partial class EventHereticResolveStarGazer : EntityEventArgs;

[DataDefinition]
public sealed partial class EventHereticAddKnowledge : EntityEventArgs
{
    [DataField(required: true)]
    public List<ProtoId<HereticKnowledgePrototype>> Knowledge;
}
