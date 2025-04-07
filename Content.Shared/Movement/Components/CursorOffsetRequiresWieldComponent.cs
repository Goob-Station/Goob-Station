// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Wieldable;
using Robust.Shared.GameStates;

namespace Content.Shared.Movement.Components;

/// <summary>
/// Indicates that this item requires wielding for the cursor offset effect to be active.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedWieldableSystem))]
public sealed partial class CursorOffsetRequiresWieldComponent : Component
{

}