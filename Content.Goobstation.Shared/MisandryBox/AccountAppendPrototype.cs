// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using System.Collections.Generic;
using Content.Goobstation.Shared.Serialization;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Goobstation.Shared.MisandryBox;

[Prototype("AccountAppend")]
public sealed class AccountAppendPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private init; } = null!;

    [DataField("userid", customTypeSerializer: typeof(GuidSerializer))]
    public Guid Userid { get; private init; } = Guid.Empty;

    // I am not dragging the whole compReg for this
    [DataField("AppendComps")]
    public List<string> Components { get; init; } = [];
}