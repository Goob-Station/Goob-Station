// SPDX-FileCopyrightText: 2021 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 ike709 <ike709@github.com>
// SPDX-FileCopyrightText: 2021 ike709 <ike709@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 EmoGarbage404 <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Veritius <veritiusgaming@gmail.com>
// SPDX-FileCopyrightText: 2022 drakewill <drake@drakewill-crl>
// SPDX-FileCopyrightText: 2022 drakewill-CRL <46307022+drakewill-CRL@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 beck-thompson <107373427+beck-thompson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Verbs
{
    /// <summary>
    ///     Contains combined name and icon information for a verb category.
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class VerbCategory
    {
        public readonly string Text;

        public readonly SpriteSpecifier? Icon;

        /// <summary>
        ///     Columns for the grid layout that shows the verbs in this category. If <see cref="IconsOnly"/> is false,
        ///     this should very likely be set to 1.
        /// </summary>
        public int Columns = 1;

        /// <summary>
        ///     If true, the members of this verb category will be shown in the context menu as a row of icons without
        ///     any text.
        /// </summary>
        /// <remarks>
        ///     For example, the 'Rotate' category simply shows two icons for rotating left and right.
        /// </remarks>
        public readonly bool IconsOnly;

        public VerbCategory(string text, string? icon, bool iconsOnly = false)
        {
            Text = Loc.GetString(text);
            Icon = icon == null ? null : new SpriteSpecifier.Texture(new(icon));
            IconsOnly = iconsOnly;
        }

        public static readonly VerbCategory Admin =
            new("verb-categories-admin", "/Textures/Interface/character.svg.192dpi.png");

        public static readonly VerbCategory Antag =
            new("verb-categories-antag", "/Textures/Interface/VerbIcons/antag-e_sword-temp.192dpi.png", iconsOnly: true) { Columns = 5 };

        public static readonly VerbCategory Examine =
            new("verb-categories-examine", "/Textures/Interface/VerbIcons/examine.svg.192dpi.png");

        public static readonly VerbCategory Debug =
            new("verb-categories-debug", "/Textures/Interface/VerbIcons/debug.svg.192dpi.png");

        public static readonly VerbCategory Eject =
            new("verb-categories-eject", "/Textures/Interface/VerbIcons/eject.svg.192dpi.png");

        public static readonly VerbCategory Insert =
            new("verb-categories-insert", "/Textures/Interface/VerbIcons/insert.svg.192dpi.png");

        public static readonly VerbCategory Buckle =
            new("verb-categories-buckle", "/Textures/Interface/VerbIcons/buckle.svg.192dpi.png");

        public static readonly VerbCategory Unbuckle =
            new("verb-categories-unbuckle", "/Textures/Interface/VerbIcons/unbuckle.svg.192dpi.png");

        public static readonly VerbCategory Rotate =
            new("verb-categories-rotate", "/Textures/Interface/VerbIcons/refresh.svg.192dpi.png", iconsOnly: true) { Columns = 5 };

        public static readonly VerbCategory Smite =
            new("verb-categories-smite", "/Textures/Interface/VerbIcons/smite.svg.192dpi.png", iconsOnly: true) { Columns = 6 };
        public static readonly VerbCategory Tricks =
            new("verb-categories-tricks", "/Textures/Interface/AdminActions/tricks.png", iconsOnly: true) { Columns = 5 };

        public static readonly VerbCategory SetTransferAmount =
            new("verb-categories-transfer", "/Textures/Interface/VerbIcons/spill.svg.192dpi.png");

        public static readonly VerbCategory Split =
            new("verb-categories-split", null);

        public static readonly VerbCategory InstrumentStyle =
            new("verb-categories-instrument-style", null);

        public static readonly VerbCategory ChannelSelect = new("verb-categories-channel-select", null);

        public static readonly VerbCategory SetSensor = new("verb-categories-set-sensor", null);

        public static readonly VerbCategory Lever = new("verb-categories-lever", null);

        public static readonly VerbCategory SelectType = new("verb-categories-select-type", null);

        public static readonly VerbCategory PowerLevel = new("verb-categories-power-level", null);

        // Shitmed - Starlight Abductors
        public static readonly VerbCategory Switch = new("verb-categories-switch", "/Textures/Interface/VerbIcons/group.svg.192dpi.png");
    }
}