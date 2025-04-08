// SPDX-FileCopyrightText: 2025 ActiveMammmoth <140334666+ActiveMammmoth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 ActiveMammmoth <kmcsmooth@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 keronshb <54602815+keronshb@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Boomerang;

[NetworkedComponent, RegisterComponent, AutoGenerateComponentState]
public sealed partial class BoomerangComponent : Component
{
    /// <summary>
    /// The entity that threw this boomerang
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Thrower;

    [DataField, AutoNetworkedField]
    public TimeSpan? TimeToReturn = TimeSpan.Zero;


    [DataField, AutoNetworkedField]
    public bool SendBack = false;
}