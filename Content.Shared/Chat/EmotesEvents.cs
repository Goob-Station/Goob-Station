// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Morb <14136326+Morb0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chat.Prototypes;
using Content.Shared.Inventory;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Chat;

/// <summary>
/// An event raised just before an emote is performed, providing systems with an opportunity to cancel the emote's performance.
/// </summary>
[ByRefEvent]
public sealed class BeforeEmoteEvent(EntityUid source, EmotePrototype emote)
    : CancellableEntityEventArgs, IInventoryRelayEvent
{
    public readonly EntityUid Source = source;
    public readonly EmotePrototype Emote = emote;

    /// <summary>
    /// The equipment that is blocking emoting. Should only be non-null if the event was canceled.
    /// </summary>
    public EntityUid? Blocker = null;

    public SlotFlags TargetSlots => SlotFlags.WITHOUT_POCKET;
}

/// <summary>
/// Raised by the chat system when an entity made some emote.
/// Use it to play sound, change sprite or something else.
/// </summary>
[ByRefEvent]
public record struct EmoteEvent(EmotePrototype Emote, bool Voluntary) // Goob - emotespam
{
    /// <summary>
    /// The used emote.
    /// </summary>
    public EmotePrototype Emote = Emote;

    /// <summary>
    /// If this message has already been "handled" by a previous system.
    /// </summary>
    public bool Handled;

    public bool Voluntary = Voluntary; // Goob - emotespam
}

/// <summary>
/// Sent by the client when requesting the server to play a specific emote selected from the emote radial menu.
/// </summary>
[Serializable, NetSerializable]
public sealed class PlayEmoteMessage(ProtoId<EmotePrototype> protoId) : EntityEventArgs
{
    public readonly ProtoId<EmotePrototype> ProtoId = protoId;
}
