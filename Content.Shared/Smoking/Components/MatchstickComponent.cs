// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

// Shitmed Change Start

using Content.Shared.Smoking.Systems;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.Smoking.Components;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedMatchstickSystem))]
[AutoGenerateComponentState]
public sealed partial class MatchstickComponent : Component
{
    /// <summary>
    /// Current state to matchstick. Can be <code>Unlit</code>, <code>Lit</code> or <code>Burnt</code>.
    /// </summary>
    [DataField("state"), AutoNetworkedField]
    public SmokableState CurrentState = SmokableState.Unlit;

    /// <summary>
    /// How long will matchstick last in seconds.
    /// </summary>
    [DataField]
    public int Duration = 10;

    /// <summary>
    /// Sound played when you ignite the matchstick.
    /// </summary>
    [DataField(required: true)]
    public SoundSpecifier IgniteSound = default!;
}

// Shitmed Change End