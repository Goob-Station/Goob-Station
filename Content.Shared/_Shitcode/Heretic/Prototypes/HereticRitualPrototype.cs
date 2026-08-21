// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Tag;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Heretic.Prototypes;

[Prototype("hereticRitual")]
public sealed partial class HereticRitualPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    /// <summary>
    ///     How many entities ritual can create at once. less or equal than 0 means no limit.
    /// </summary>
    [DataField]
    public int Limit;

    /// <summary>
    ///     How many entitites with specific names are required for the ritual?
    /// </summary>
    [DataField] public Dictionary<string, int>? RequiredEntityNames;

    /// <summary>
    ///     How many items with certain tags are required for the ritual?
    /// </summary>
    [DataField] public Dictionary<ProtoId<TagPrototype>, int>? RequiredTags;

    /// <summary>
    ///     Is there a custom behavior that needs to be executed?
    /// </summary>
    [DataField] public List<RitualCustomBehavior>? CustomBehaviors;

    /// <summary>
    ///     How many other entities will be created from the ritual?
    /// </summary>
    [DataField] public Dictionary<EntProtoId, int>? Output;
    /// <summary>
    ///     What event will be raised on success?
    /// </summary>
    [DataField] public object? OutputEvent;
    /// <summary>
    ///     What knowledge will be given on success?
    /// </summary>
    [DataField] public ProtoId<HereticKnowledgePrototype>? OutputKnowledge;

    /// <summary>
    ///     Used for codex and radial menu.
    /// </summary>
    [DataField] public string LocName = "heretic-ritual-unknown";

    /// <summary>
    ///     Used for codex
    /// </summary>
    [DataField] public string LocDesc = string.Empty;

    /// <summary>
    ///     Icon for codex and radial menu.
    /// </summary>
    [DataField] public SpriteSpecifier Icon = new SpriteSpecifier.Rsi(new("_Goobstation/Heretic/amber_focus.rsi"), "icon");

    /// <summary>
    ///     Whether rune should play success animation on ritual success.
    /// </summary>
    [DataField]
    public bool RuneSuccessAnimation = true;
}
