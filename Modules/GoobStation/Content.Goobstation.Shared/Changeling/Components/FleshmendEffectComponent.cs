// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Marcus F <199992874+thebiggestbruh@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Changeling.Components;

/// <summary>
///     Component responsible for Fleshmend's visual effects.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class FleshmendEffectComponent : Component
{
    public string EffectState = "mend_active";

    public ResPath ResPath = new("_Goobstation/Changeling/fleshmend_visuals.rsi");

}

public enum FleshmendEffectKey : byte
{
    Key,
}
