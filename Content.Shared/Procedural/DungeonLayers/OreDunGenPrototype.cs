// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.Prototypes;

namespace Content.Shared.Procedural.DungeonLayers;

[Prototype]
public sealed partial class OreDunGenPrototype : OreDunGen, IPrototype
{
    [IdDataField]
    public string ID { set; get; } = default!;
}
