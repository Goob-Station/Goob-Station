// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Robust.Shared.Serialization;

namespace Content.Shared._Shitmed.Switchable;

public sealed partial class SwitchableActionEvent : InstantActionEvent;

/// <summary>
///     Generic enum keys for toggle-visualizer appearance data & sprite layers.
/// </summary>
[Serializable, NetSerializable]
public enum SwitchableVisuals : byte
{
    Switched,
    Layer
}