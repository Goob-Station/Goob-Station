// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Heretic;
using Robust.Shared.Prototypes;

namespace Content.Shared.Heretic.Prototypes;

[Prototype("hereticKnowledge")]
public sealed partial class HereticKnowledgePrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    [DataField] public string? Path;

    [DataField] public int Stage = 1;

    /// <summary>
    ///     Indicates that this should not be on a main branch.
    /// </summary>
    [DataField] public bool SideKnowledge = false;

    /// <summary>
    ///     What event should be raised
    /// </summary>
    [DataField, NonSerialized] public HereticKnowledgeEvent? Event;

    /// <summary>
    ///     What rituals should be given
    /// </summary>
    [DataField] public List<ProtoId<HereticRitualPrototype>>? RitualPrototypes;

    /// <summary>
    ///     What actions should be given
    /// </summary>
    [DataField] public List<EntProtoId>? ActionPrototypes;

    /// <summary>
    ///     Used for codex
    /// </summary>
    [DataField] public string LocName = string.Empty;

    /// <summary>
    ///     Used for codex
    /// </summary>
    [DataField] public string LocDesc = string.Empty;
}
