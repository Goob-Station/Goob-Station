// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 TsjipTsjip <19798667+TsjipTsjip@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Utility;
using Robust.Shared.Audio; // Goobstation - Prayer Sound

namespace Content.Shared.Prayer;

/// <summary>
/// Allows an entity to be prayed on in the context menu
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PrayableComponent : Component
{
    /// <summary>
    /// If bible users are only allowed to use this prayable entity
    /// </summary>
    [DataField("bibleUserOnly")]
    [ViewVariables(VVAccess.ReadWrite)]
    public bool BibleUserOnly;

    /// <summary>
    /// Message given to user to notify them a message was sent
    /// </summary>
    [DataField("sentMessage")]
    [ViewVariables(VVAccess.ReadWrite)]
    public string SentMessage = "prayer-popup-notify-pray-sent";

    /// <summary>
    /// Prefix used in the notification to admins
    /// </summary>
    [DataField("notificationPrefix")]
    [ViewVariables(VVAccess.ReadWrite)]
    public string NotificationPrefix = "prayer-chat-notify-pray";

    /// <summary>
    /// Used in window title and context menu
    /// </summary>
    [DataField("verb")]
    [ViewVariables(VVAccess.ReadOnly)]
    public string Verb = "prayer-verbs-pray";

    /// <summary>
    /// Context menu image
    /// </summary>
    [DataField("verbImage")]
    [ViewVariables(VVAccess.ReadOnly)]
    public SpriteSpecifier? VerbImage = new SpriteSpecifier.Texture(new ("/Textures/Interface/pray.svg.png"));

    // Goobstation - Prayer sound
    /// <summary>
    /// Optional sound played with the admin notification.
    /// </summary>
    [DataField]
    public SoundSpecifier? NotificationSound = new SoundPathSpecifier("/Audio/Effects/holy.ogg");
}
