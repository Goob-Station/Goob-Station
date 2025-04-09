// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Materials;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Heretic.Prototypes;

[Serializable, NetSerializable, DataDefinition]
[Prototype("hereticRitual")]
public sealed partial class HereticRitualPrototype : IPrototype, ICloneable
{
    [IdDataField] public string ID { get; private set; } = default!;

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

    /// <remarks> Please use this instead of editing the prototype. Shit WILL break if you don't. </remarks>
    public object Clone()
    {
        return new HereticRitualPrototype()
        {
            ID = ID,
            RequiredEntityNames = RequiredEntityNames,
            RequiredTags = RequiredTags,
            CustomBehaviors = CustomBehaviors,
            Output = Output,
            OutputEvent = OutputEvent,
            OutputKnowledge = OutputKnowledge,
            LocName = LocName,
            LocDesc = LocDesc,
            Icon = Icon
        };
    }
}