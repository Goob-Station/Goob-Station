// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿using Robust.Shared.Prototypes;

namespace Content.Shared.Chemistry.Reaction;

[Prototype]
public sealed partial class ReactiveGroupPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;
}
