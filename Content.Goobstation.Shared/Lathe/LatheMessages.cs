// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Soup-Byte07 <soupbyte30@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Lathe;

/// <summary>
///     Sent to the server when a client resets the queue
/// </summary>
[Serializable, NetSerializable]
public sealed class LatheQueueResetMessage : BoundUserInterfaceMessage;
