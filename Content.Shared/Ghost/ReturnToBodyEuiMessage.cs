// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared.Ghost;

[Serializable, NetSerializable]
public sealed class ReturnToBodyMessage : EuiMessageBase
{
    public readonly bool Accepted;

    public ReturnToBodyMessage(bool accepted)
    {
        Accepted = accepted;
    }
}
