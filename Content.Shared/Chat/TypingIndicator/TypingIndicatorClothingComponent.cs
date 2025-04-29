// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Morb <14136326+Morb0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 beck-thompson <107373427+beck-thompson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Chat.TypingIndicator;

/// <summary>
///     If an item is equipped to someones inventory (Anything but the pockets), and has this component
///     the users typing indicator will be replaced by the prototype given in <c>TypingIndicatorPrototype</c>.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
[Access(typeof(SharedTypingIndicatorSystem))]
public sealed partial class TypingIndicatorClothingComponent : Component
{
    /// <summary>
    ///     The typing indicator that will override the default typing indicator when the item is equipped to a users
    ///     inventory.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("proto", required: true)]
    public ProtoId<TypingIndicatorPrototype> TypingIndicatorPrototype = default!;

    /// <summary>
    ///     This stores the time the item was equipped in someones inventory. If null, item is currently not equipped.
    /// </summary>
    [DataField, AutoPausedField]
    public TimeSpan? GotEquippedTime = null;
}