// SPDX-FileCopyrightText: 2024 ScarKy0 <106310278+ScarKy0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

//using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared.Silicons.Borgs.Components;

/// <summary>
/// This is used to override the action icon for cyborg actions.
/// Without this component the no-action state will be used.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BorgModuleIconComponent : Component
{
    /// <summary>
    /// The action icon for this module
    /// </summary>
    [DataField]
    public SpriteSpecifier.Rsi Icon = default!;

}