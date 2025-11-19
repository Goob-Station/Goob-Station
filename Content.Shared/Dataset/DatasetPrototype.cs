// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿using Robust.Shared.Prototypes;

namespace Content.Shared.Dataset
{
    [Prototype]
    public sealed partial class DatasetPrototype : IPrototype
    {
        [ViewVariables]
        [IdDataField]
        public string ID { get; private set; } = default!;

        [DataField("values")] public IReadOnlyList<string> Values { get; private set; } = new List<string>();
    }
}
