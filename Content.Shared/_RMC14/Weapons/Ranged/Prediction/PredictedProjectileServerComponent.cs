// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Shared._RMC14.Weapons.Ranged.Prediction;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PredictedProjectileServerComponent : Component
{
    public ICommonSession? Shooter;

    [DataField, AutoNetworkedField]
    public int ClientId;

    [DataField, AutoNetworkedField]
    public EntityUid? ClientEnt;

    [DataField]
    public bool Hit;
}
