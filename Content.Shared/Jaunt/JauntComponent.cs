// SPDX-FileCopyrightText: 2024 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Jaunt;

/// <summary>
///     Used to control various aspects of a Jaunt.
///     Can be used in place of giving a jaunt-action directly.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class JauntComponent : Component
{
    /// <summary>
    ///     Which Jaunt Action the component should grant.
    /// </summary>
    [DataField]
    public EntProtoId JauntAction = "ActionPolymorphJaunt";

    /// <summary>
    ///     The jaunt action itself.
    /// </summary>
    public EntityUid? Action;

    // TODO: Enter & Exit Times and Whitelist when Actions are reworked and can support it
    // TODO: Cooldown pausing when Actions can support it
}