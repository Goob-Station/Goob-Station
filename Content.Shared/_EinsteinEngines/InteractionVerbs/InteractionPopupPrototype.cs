// SPDX-FileCopyrightText: 2024 Mnemotechnican <69920617+Mnemotechnician@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 RadsammyT <radsammyt@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Chat;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;

namespace Content.Shared.InteractionVerbs;

/// <summary>
///     Specifies how popups should be shown.<br/>
///     Popup locales follow the format "interaction-[verb id]-[prefix]-[kind suffix]-popup", where: <br/>
///     - [prefix] is <see cref="Prefix"/>, which is one of: "success", "fail", "delayed". <br/>
///     - [kind suffix] is one of the respective suffix properties, typically "self", "target", or "others" <br/>
/// </summary>
/// <remarks>
///     The following parameters may be used in the locale: <br/>
///     - {$user} - The performer of the action. <br/>
///     - {$target} - The target of the action. <br/>
///     - {$used} - The active-hand item used in the action. May be null, then "0" is used instead.
///     - {$selfTarget} - A boolean value that indicates whether the action is used on the user itself.
///     - {$hasUsed} - A boolean value that indicates whether the user is holding an item ($used is not null).
/// </remarks>
[Prototype("InteractionPopup")]
public sealed partial class InteractionPopupPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public PopupType PopupType = PopupType.Medium;

    /// <summary>
    ///     If true, the respective success/fail popups will be logged into chat.
    /// </summary>
    [DataField]
    public bool LogPopup = true;

    /// <summary>
    ///     Chat channel to which popups will be logged.
    /// </summary>
    [DataField]
    public ChatChannel LogChannel = ChatChannel.Emotes;

    /// <summary>
    ///     Color of the chat message sent.
    /// </summary>
    [DataField]
    public Color? LogColor = null;

    /// <summary>
    ///     If true, entities who cannot directly see the popup target will not chat log.
    /// </summary>
    [DataField]
    public bool DoClipping = true;

    /// <summary>
    ///     Range in which other entities can see the chat log.
    /// </summary>
    [DataField]
    public float VisibilityRange = 20f;

    /// <summary>
    ///     Popup suffix for the performer.
    /// </summary>
    [DataField("self")]
    public string? SelfSuffix = "self";

    /// <summary>
    ///     Popup suffix for the target.
    /// </summary>
    [DataField("target")]
    public string? TargetSuffix = "target";

    /// <summary>
    ///     Popup suffix for observers.
    /// </summary>
    [DataField("others")]
    public string? OthersSuffix = "others";

    public enum Prefix : byte
    {
        Success,
        Fail,
        Delayed
    }
}
