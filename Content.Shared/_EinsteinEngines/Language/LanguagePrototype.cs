// SPDX-FileCopyrightText: 2025 CerberusWolfie <wb.johnb.willis@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 John Willis <143434770+CerberusWolfie@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Mnemotechnican <69920617+Mnemotechnician@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chat;
using Robust.Shared.Prototypes;

namespace Content.Shared._EinsteinEngines.Language;

[Prototype("language")]
public sealed partial class LanguagePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    ///     Whether this language will display its name in chat behind a player's name.
    /// </summary>
    [DataField]
    public bool IsVisibleLanguage { get; set; }

    /// <summary>
    ///     Obfuscation method used by this language. By default, uses <see cref="ObfuscationMethod.Default"/>.
    /// </summary>
    [DataField("obfuscation")]
    public ObfuscationMethod Obfuscation = ObfuscationMethod.Default;

    /// <summary>
    ///     Speech overrides used for messages sent in this language.
    /// </summary>
    [DataField("speech")]
    public SpeechOverrideInfo SpeechOverride = new();

    #region utility
    /// <summary>
    ///     The in-world name of this language, localized.
    /// </summary>
    public string Name => Loc.GetString($"language-{ID}-name");

    /// <summary>
    ///     The in-world chat abbreviation of this language, localized.
    /// </summary>
    public string ChatName => Loc.GetString($"chat-language-{ID}-name");

    /// <summary>
    ///     The in-world description of this language, localized.
    /// </summary>
    public string Description => Loc.GetString($"language-{ID}-description");
    #endregion utility
}

[DataDefinition]
public sealed partial class SpeechOverrideInfo
{
    /// <summary>
    ///     Color which text in this language will be blended with.
    ///     Alpha blending is used, which means the alpha component of the color controls the intensity of this color.
    /// </summary>
    [DataField]
    public Color? Color = null;

    [DataField]
    public string? FontId;

    [DataField]
    public int? FontSize;

    [DataField]
    public string? BoldFontId; // Goob Edit - Custom Bolded Fonts

    [DataField]
    public bool AllowRadio = true;

    /// <summary>
    ///     If true, the message will be relayed to the Empathy Chat and
    ///     anyone with that language will also hear Empathy Chat. (Unless user has ShadowkinBlackeyeComponent)
    ///     This is mostly only use for "Marish" but... fuckit modularity :p
    /// </summary>
    [DataField]
    public bool EmpathySpeech = false;

    /// <summary>
    ///     If false, the entity can use this language even when it's unable to speak (i.e. muffled or muted),
    ///     and accents are not applied to messages in this language.
    /// </summary>
    [DataField]
    public bool RequireSpeech = true;

    /// <summary>
    ///     If true, the listener must have a line of sight on the speaker to hear the message.
    /// </summary>
    [DataField]
    public bool RequireLOS = false; // Floofstation - Check Line-Of-Sight

    /// <summary>
    ///     If not null, all messages in this language will be forced to be spoken in this chat type.
    /// </summary>
    [DataField]
    public InGameICChatType? ChatTypeOverride;

    /// <summary>
    ///     Speech verb overrides. If not provided, the default ones for the entity are used.
    /// </summary>
    [DataField]
    public List<LocId>? SpeechVerbOverrides;

    /// <summary>
    ///     Overrides for different kinds chat message wraps. If not provided, the default ones are used.
    /// </summary>
    /// <remarks>
    ///     Currently, only local chat and whispers support this. Radio and emotes are unaffected.
    ///     This is horrible.
    /// </remarks>
    [DataField]
    public Dictionary<InGameICChatType, LocId> MessageWrapOverrides = new();
}
