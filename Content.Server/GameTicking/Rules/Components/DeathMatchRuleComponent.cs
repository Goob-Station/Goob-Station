// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.FixedPoint;
using Content.Shared.Roles;
using Content.Shared.Storage;
using Robust.Shared.Network;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.GameTicking.Rules.Components;

/// <summary>
/// Gamerule that ends when a player gets a certain number of kills.
/// </summary>
[RegisterComponent, Access(typeof(DeathMatchRuleSystem))]
public sealed partial class DeathMatchRuleComponent : Component
{
    /// <summary>
    /// The number of points a player has to get to win.
    /// </summary>
    [DataField("killCap"), ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 KillCap = 31;

    /// <summary>
    /// How long until the round restarts
    /// </summary>
    [DataField("restartDelay"), ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan RestartDelay = TimeSpan.FromSeconds(10f);

    /// <summary>
    /// The person who won.
    /// We store this here in case of some assist shenanigans.
    /// </summary>
    [DataField("victor")]
    public NetUserId? Victor;

    /// <summary>
    /// An entity spawned after a player is killed.
    /// </summary>
    [DataField("rewardSpawns")]
    public List<EntitySpawnEntry> RewardSpawns = new();

    /// <summary>
    /// The gear all players spawn with.
    /// </summary>
    [DataField("gear", customTypeSerializer: typeof(PrototypeIdSerializer<StartingGearPrototype>)), ViewVariables(VVAccess.ReadWrite)]
    public string Gear = "DeathMatchGear";
}