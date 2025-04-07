// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

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