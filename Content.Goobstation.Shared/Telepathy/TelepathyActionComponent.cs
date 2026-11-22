// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Popups;
using Content.Shared.Whitelist;

namespace Content.Goobstation.Shared.Telepathy;

/// <summary>
/// This is used for data about Telepathy actions, such as slaughter demon whisper.
/// </summary>
[RegisterComponent]
public sealed partial class TelepathyActionComponent : Component
{
    /// <summary>
    /// Whitelist of entities that the telepathy can be used on.
    /// </summary>
    [DataField]
    public EntityWhitelist? TargetWhitelist;

    /// <summary>
    /// Popup type for the telepathy, eg. MediumCaution for evil
    /// </summary>
    [DataField]
    public PopupType PopupType = PopupType.Medium;

    /// <summary>
    /// Locale for title of the telepathy dialogue.
    /// </summary>
    [DataField]
    public LocId DialogueTitle = "telepathic-whisper-title";

    /// <summary>
    /// Locale for message to self when whispering.
    /// </summary>
    [DataField]
    public LocId PopupWhisperSelf = "telepathic-whisper-self";

    /// <summary>
    /// Extra flavor text when whispering to target, eg. Suddenly, a voice resonates in your head...
    /// </summary>
    [DataField]
    public LocId? PopupWhisperFlavor;
};
