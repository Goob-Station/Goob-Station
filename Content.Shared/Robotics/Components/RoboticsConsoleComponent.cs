// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Radio;
using Content.Shared.Robotics.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Robotics.Components;

/// <summary>
/// Robotics console for managing borgs.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedRoboticsConsoleSystem))]
[AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class RoboticsConsoleComponent : Component
{
    /// <summary>
    /// Address and data of each cyborg.
    /// </summary>
    [DataField]
    public Dictionary<string, CyborgControlData> Cyborgs = new();

    /// <summary>
    /// After not responding for this length of time borgs are removed from the console.
    /// </summary>
    [DataField]
    public TimeSpan Timeout = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Radio channel to send messages on.
    /// </summary>
    [DataField]
    public ProtoId<RadioChannelPrototype> RadioChannel = "Science";

    /// <summary>
    /// Radio message sent when destroying a borg.
    /// </summary>
    [DataField]
    public LocId DestroyMessage = "robotics-console-cyborg-destroying";

    /// <summary>
    /// Cooldown on destroying borgs to prevent complete abuse.
    /// </summary>
    [DataField]
    public TimeSpan DestroyCooldown = TimeSpan.FromSeconds(30);

    /// <summary>
    /// When a borg can next be destroyed.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan NextDestroy = TimeSpan.Zero;
}