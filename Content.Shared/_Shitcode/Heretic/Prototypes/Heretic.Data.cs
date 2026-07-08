// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Heretic.Prototypes;



[Serializable, NetSerializable, DataDefinition] public sealed partial class EventHereticAscension : EntityEventArgs { }
[Serializable, NetSerializable, DataDefinition] public sealed partial class EventHereticRerollTargets : EntityEventArgs { }
[Serializable, NetSerializable, DataDefinition] public sealed partial class EventHereticUpdateTargets : EntityEventArgs { }
