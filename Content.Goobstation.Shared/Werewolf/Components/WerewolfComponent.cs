// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Werewolf.Components;

/// <summary>
/// This is the main component of the werewolf
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class WerewolfComponent : Component
{
    /// <summary>
    /// The currency that the werewolves have for buying forms.
    /// Fury can be gained through eating humanoid organs, while in werewolf form.
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public int Fury;
}
