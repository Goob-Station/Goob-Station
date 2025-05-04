// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.RelayedDeathrattle;

[RegisterComponent]
public sealed partial class RelayedDeathrattleComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid? Target;

    [DataField]
    public LocId CritMessage = "deathrattle-implant-critical-message";

    [DataField]
    public LocId DeathMessage = "deathrattle-implant-dead-message";

    [DataField]
    public bool RequireCrewMonitor = true;
}
