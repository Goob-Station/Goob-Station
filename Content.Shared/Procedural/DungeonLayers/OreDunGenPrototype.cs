// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared.Procedural.DungeonLayers;

[Prototype]
public sealed partial class OreDunGenPrototype : OreDunGen, IPrototype
{
    [IdDataField]
    public string ID { set; get; } = default!;
}