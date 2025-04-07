// SPDX-FileCopyrightText: 2024 beck-thompson <107373427+beck-thompson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared.ChangeNameInContainer;

/// <summary>
///     An entity with this component will get its name and verb chaned to the container it's inside of. E.g, if your a
///     pAI that has this component and are inside a lizard plushie, your name when talking will be "lizard plushie".
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(ChangeNameInContainerSystem))]
public sealed partial class ChangeVoiceInContainerComponent : Component
{
    /// <summary>
    ///     A whitelist of containers that will change the name.
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist;
}