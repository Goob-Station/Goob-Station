// SPDX-FileCopyrightText: 2023 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Chat;

using Content.Server.Chat.Systems;
using Content.Shared.Chat.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

/// <summary>
/// Causes an entity to automatically emote when taking damage.
/// </summary>
[RegisterComponent, Access(typeof(EmoteOnDamageSystem)), AutoGenerateComponentPause]
public sealed partial class EmoteOnDamageComponent : Component
{
    /// <summary>
    /// Chance of preforming an emote when taking damage and not on cooldown.
    /// </summary>
    [DataField("emoteChance"), ViewVariables(VVAccess.ReadWrite)]
    public float EmoteChance = 0.5f;

    /// <summary>
    /// A dictionary of emotes threshold that will be randomly picked from.
    /// <see cref="EmotePrototype"/>
    /// </summary>
    [DataField("emotes"), ViewVariables(VVAccess.ReadWrite)]
    public Dictionary<float, HashSet<string>> EmotesThreshold = new(); // CorvaxGoob-AutoEmote : Changes

    /// <summary>
    /// Also send the emote in chat.
    /// <summary>
    [DataField("withChat"), ViewVariables(VVAccess.ReadWrite)]
    public bool WithChat = false;

    /// <summary>
    /// Hide the chat message from the chat window, only showing the popup.
    /// This does nothing if WithChat is false.
    /// <summary>
    [DataField("hiddenFromChatWindow")]
    public bool HiddenFromChatWindow = false;

    /// <summary>
    /// The simulation time of the last emote preformed due to taking damage.
    /// </summary>
    [DataField("lastEmoteTime", customTypeSerializer: typeof(TimeOffsetSerializer)), ViewVariables(VVAccess.ReadWrite)]
    [AutoPausedField]
    public TimeSpan LastEmoteTime = TimeSpan.Zero;

    /// <summary>
    /// The cooldown between emotes.
    /// </summary>
    [DataField("emoteCooldown"), ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan EmoteCooldown = TimeSpan.FromSeconds(2);

    // CorvaxGoob-AutoEmote-Start : Changes
    [DataField]
    public HashSet<string> AllowedDamageType = ["Blunt", "Caustic", "Heat", "Cold", "Piercing", "Shock", "Slash"];

    [DataField]
    public float PainThreshold = 6.0f;
    // CorvaxGoob-AutoEmote-End : Changes
}
