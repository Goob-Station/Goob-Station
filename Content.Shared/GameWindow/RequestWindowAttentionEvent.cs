// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿using Robust.Shared.Serialization;

namespace Content.Shared.GameWindow
{
    [Serializable, NetSerializable]
    public sealed class RequestWindowAttentionEvent : EntityEventArgs
    {
    }
}
