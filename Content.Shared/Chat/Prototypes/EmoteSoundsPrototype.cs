// SPDX-FileCopyrightText: 2023 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Morb <14136326+Morb0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tayrtahn <tayrtahn@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;

namespace Content.Shared.Chat.Prototypes;

/// <summary>
///     Sounds collection for each <see cref="EmotePrototype"/>.
///     Different entities may use different sounds collections.
/// </summary>
[Prototype]
public sealed partial class EmoteSoundsPrototype : IPrototype, IInheritingPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <inheritdoc/>
    [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<EmoteSoundsPrototype>))]
    public string[]? Parents { get; private set; }

    /// <inheritdoc/>
    [AbstractDataField]
    [NeverPushInheritance]
    public bool Abstract { get; private set; }

    /// <summary>
    ///     Optional fallback sound that will play if collection
    ///     doesn't have specific sound for this emote id.
    /// </summary>
    [DataField("sound")]
    public SoundSpecifier? FallbackSound;

    /// <summary>
    ///     Optional audio params that will be applied to ALL sounds.
    ///     This will overwrite any params that may be set in sound specifiers.
    /// </summary>
    [DataField("params")]
    public AudioParams? GeneralParams;

    /// <summary>
    ///     Collection of emote prototypes and their sounds.
    /// </summary>
    [DataField]
    [AlwaysPushInheritance]
    public Dictionary<ProtoId<EmotePrototype>, SoundSpecifier> Sounds = new();
}
