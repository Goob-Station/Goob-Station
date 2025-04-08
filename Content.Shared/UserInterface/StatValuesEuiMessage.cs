// SPDX-FileCopyrightText: 2022 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared.UserInterface;

/// <summary>
/// It's a message not a state because it's for debugging and it makes it easier to bootstrap more data dumping.
/// </summary>
[Serializable, NetSerializable]
public sealed class StatValuesEuiMessage : EuiMessageBase
{
    /// <summary>
    /// Titles for the window.
    /// </summary>
    public string Title = string.Empty;
    public List<string> Headers = new();
    public List<string[]> Values = new();
}