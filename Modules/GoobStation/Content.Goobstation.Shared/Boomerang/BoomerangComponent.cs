// SPDX-FileCopyrightText: 2025 ActiveMammmoth <140334666+ActiveMammmoth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 ActiveMammmoth <kmcsmooth@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 keronshb <54602815+keronshb@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Boomerang;

/// <summary>
/// Component for entities that should boomerang back to their thrower once thrown
/// </summary>
[NetworkedComponent, RegisterComponent, AutoGenerateComponentState]
public sealed partial class BoomerangComponent : Component
{
    /// <summary>
    /// Entity we should return to after landing
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Thrower;

    /// <summary>
    /// Distance to thrower we should try get picked up at or fail
    /// </summary>
    [DataField]
    public float PickupDistance = 1.5f;

    /// <summary>
    /// Speed we should return at
    /// </summary>
    [DataField]
    public float ReturnSpeed = 10f;

    /// <summary>
    /// Maximum return hops we can make
    /// </summary>
    [DataField]
    public int MaxHops = 6;

    /// <summary>
    /// Return hops we've made so far
    /// </summary>
    [DataField, AutoNetworkedField]
    public int CurrentHops = 0;
}
