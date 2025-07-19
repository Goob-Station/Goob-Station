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
[Prototype("InteractionPopup"), Serializable]
public sealed partial class InteractionPopupPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    [DataField]
    public PopupType PopupType = PopupType.Medium;

    /// <summary>
    ///     If true, the respective success/fail popups will be logged into chat, as players perceive them.
    /// </summary>
    [DataField]
    public bool LogPopup = true;

    /// <summary>
    ///     Chat channel to which popups will be logged if <see cref="LogPopup"/> is true.
    /// </summary>
    [DataField]
    public ChatChannel LogChannel = ChatChannel.Emotes;

    /// <summary>
    ///     Color of the chat message sent if <see cref="LogPopup"/> is true. If null, defaults based on <see cref="Type"/>.
    /// </summary>
    [DataField]
    public Color? LogColor = null;

    /// <summary>
    ///     If true, entities who cannot directly see the popup target will not chat log. Only has effect if <see cref="LogPopup"/> is true.
    /// </summary>
    [DataField]
    public bool DoClipping = true;

    /// <summary>
    ///     Range in which other entities, given that they can directly see the performer, see the chat log.
    ///     This does not affect the user and target. Only has effect if <see cref="LogPopup"/> is true.
    /// </summary>
    [DataField]
    public float VisibilityRange = 20f;

    /// <summary>
    ///     Loc prefix for popups shown for the performer of the verb. If set to null, defaults to <see cref="OthersSuffix"/>.
    /// </summary>
    [DataField("self")]
    public string? SelfSuffix = "self";

    /// <summary>
    ///     Loc prefix for popups shown for the target of the verb. If set to null, defaults to <see cref="OthersSuffix"/>.
    /// </summary>
    [DataField("target")]
    public string? TargetSuffix = "target";

    /// <summary>
    ///     Loc prefix for popups shown for other people observing the verb. If null, no popup will be shown for others.
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
