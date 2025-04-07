// SPDX-FileCopyrightText: 2024 Milon <milonpl.git@proton.me>
// SPDX-FileCopyrightText: 2024 Skubman <ba.fallaria@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Radio;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.CartridgeLoader.Cartridges;

[RegisterComponent, Access(typeof(NanoChatCartridgeSystem))]
public sealed partial class NanoChatCartridgeComponent : Component
{
    /// <summary>
    ///     Station entity to keep track of.
    /// </summary>
    [DataField]
    public EntityUid? Station;

    /// <summary>
    ///     The NanoChat card to keep track of.
    /// </summary>
    [DataField]
    public EntityUid? Card;

    /// <summary>
    ///     The <see cref="RadioChannelPrototype" /> required to send or receive messages.
    /// </summary>
    [DataField]
    public ProtoId<RadioChannelPrototype> RadioChannel = "Common";
}