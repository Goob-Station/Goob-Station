// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Marcus F <199992874+thebiggestbruh@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.SpecialPassives.Fleshmend.Components;

/// <summary>
///     Component responsible for Fleshmend's visual effects. Should NOT be added outside of FleshmendSystem.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class FleshmendEffectComponent : Component
{
    [DataField, AutoNetworkedField]
    public string EffectState;

    [DataField, AutoNetworkedField]
    public ResPath ResPath;

}

public enum FleshmendEffectKey : byte
{
    Key,
}
