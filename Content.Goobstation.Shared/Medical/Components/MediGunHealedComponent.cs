// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Medical.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MediGunHealedComponent : Component
{
    /// <summary>
    /// Source of the healing that it receives.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid Source;

    /// <summary>
    /// Color that will be used on target entity when healing is active.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Color LineColor;
}
