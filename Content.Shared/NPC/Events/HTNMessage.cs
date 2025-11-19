// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.Serialization;

namespace Content.Shared.NPC;

/// <summary>
/// Has debug information for HTN NPCs.
/// </summary>
[Serializable, NetSerializable]
public sealed class HTNMessage : EntityEventArgs
{
    public NetEntity Uid;
    public string Text = string.Empty;
}
