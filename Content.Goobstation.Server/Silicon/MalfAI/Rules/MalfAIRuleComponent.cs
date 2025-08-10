// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 ThunderBear2006 <bearthunder06@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Explosion;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Silicon.MalfAI.Rules;

[RegisterComponent]
public sealed partial class MalfAIRuleComponent : Component
{
    /// <summary>
    /// The station this gamerule is taking place on. SHOULD be the main station.
    /// </summary>
    [DataField]
    public EntityUid Station;

    /// <summary>
    /// The malf AI this rule applies to.
    /// </summary>
    [DataField]
    public EntityUid AIEntity;

    /// <summary>
    /// Set to true when initiating the startup sequence for the doom device...
    /// ya know, the funny nerd messages then da BIG BOOM!
    /// </summary>
    [DataField]
    public bool DoomDeviceStarting = false;

    /// <summary>
    /// When this is set to true for the first time the station alert
    /// level is set to delta and the crew are notified that the doomsday
    /// device is ticking down.
    /// </summary>
    [DataField]
    public bool DoomDeviceActive = false;

    [DataField]
    public bool PlayedAlarm = false;

    [DataField]
    public TimeSpan TimeDoomDeviceStarted;

    /// <summary>
    /// How much time until the device goes off.
    /// This is compared against the last message time once
    /// no more fluff messages are left since time between 
    /// the fluff messages is randomized.
    /// </summary>
    [DataField]
    public TimeSpan TimeUntilDoomSetOff = TimeSpan.FromSeconds(14); // 450

    [DataField]
    public TimeSpan TimeUntilAlarmStart = TimeSpan.FromSeconds(1); // 437

    /// <summary>
    /// Time until the next fluff message is sent in chat 
    /// to the AI activating the device
    /// </summary>
    [DataField]
    public TimeSpan TimeUntilNextDoomDeviceMessage = TimeSpan.FromSeconds(1);

    [DataField]
    public TimeSpan LastDoomDeviceMessageTime = TimeSpan.FromSeconds(0);

    [DataField]
    public string DoomAnnouncementStartLoc = "announcement-doomsday-start";

    [DataField]
    public string OnlyOnStationLoc = "doom-device-failed-off-station";

    [DataField]
    public string OnlyOneDeviceLoc = "doom-device-failed-only-one";

    [DataField(required: true)]
    public List<string> DoomFluffMessageLocs;

    public int DoomFluffMessagesIndex = 0;

    [DataField]
    public SoundSpecifier DoomAnnouncementSoundSpecifier = new SoundPathSpecifier("/Audio/_Goobstation/Announcements/aimalf.ogg");

    [DataField]
    public SoundSpecifier FluffMessageSoundSpecifier = new SoundPathSpecifier("/Audio/Machines/quickbeep.ogg");

    [DataField]
    public SoundSpecifier AlarmSpecifier = new SoundPathSpecifier("/Audio/Machines/Nuke/nuke_alarm.ogg");
}