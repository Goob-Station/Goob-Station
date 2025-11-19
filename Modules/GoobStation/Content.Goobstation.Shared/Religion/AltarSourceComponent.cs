// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Religion.Nullrod;

    [RegisterComponent]
    public sealed partial class AltarSourceComponent : Component
    {

        /// <summary>
        /// Which prototype to create.
        /// </summary>
        [DataField]
        public EntProtoId RodProto = "Nullrod";

        /// <summary>
        /// Which effect to display.
        /// </summary>
        [DataField]
        public EntProtoId EffectProto = "EffectSpark";

        /// <summary>
        /// Which sound effect to play.
        /// </summary>
        [DataField]
        public SoundSpecifier? SoundPath;

    }
