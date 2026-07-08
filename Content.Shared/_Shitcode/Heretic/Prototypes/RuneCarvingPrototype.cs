// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Heretic.Prototypes;

[Prototype("runeCarving")]
public sealed partial class RuneCarvingPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public SpriteSpecifier Icon = default!;

    [DataField(required: true)]
    public string Desc = default!;

    [DataField(required: true)]
    public EntProtoId ProtoId;
}
