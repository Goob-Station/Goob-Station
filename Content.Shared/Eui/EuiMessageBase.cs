// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿using Robust.Shared.Serialization;

namespace Content.Shared.Eui
{
    [Serializable]
    public abstract class EuiMessageBase
    {

    }

    [Serializable, NetSerializable]
    public sealed class CloseEuiMessage : EuiMessageBase
    {
    }
}
