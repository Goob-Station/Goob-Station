// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityTable;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.SetSelector;

/// <summary>
/// A prototype that defines a set available for selection for <see>
///     <cref>SetSelectorComponent</cref>
/// </see>
/// </summary>
[Prototype]
public sealed partial class SelectableSetPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;
    [DataField] public string Name { get; private set; } = string.Empty;
    [DataField] public string Description { get; private set; } = string.Empty;
    [DataField] public SpriteSpecifier Sprite { get; private set; } = SpriteSpecifier.Invalid;
    [DataField] public List<EntProtoId> Content = new();
    [DataField] public List<ProtoId<EntityTablePrototype>> Tables = new();
}
