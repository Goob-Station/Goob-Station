// SPDX-FileCopyrightText: 2023 AJCM <AJCM@tutanota.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.HotPotato;

/// <summary>
/// Added to an activated hot potato. Controls hot potato transfer on server / effect spawning on client.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedHotPotatoSystem))]
public sealed partial class ActiveHotPotatoComponent : Component
{
    /// <summary>
    /// Hot potato effect spawn cooldown in seconds
    /// </summary>
    [DataField("effectCooldown"), ViewVariables(VVAccess.ReadWrite)]
    public float EffectCooldown = 0.3f;

    /// <summary>
    /// Moment in time next effect will be spawned
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan TargetTime = TimeSpan.Zero;
}