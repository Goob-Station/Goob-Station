// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Server.Religion;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AlternatePrayableComponent : Component
    {
        /// <summary>
        /// How long does the praying do-after take to complete?
        /// </summary>
        [DataField]
        public TimeSpan PrayDoAfterDuration = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Should the prayer be repeated endlessly until cancelled?
        /// </summary>
        [DataField]
        public bool RepeatPrayer;

        /// <summary>
        /// Does the user have to be a bible user to pray at this?
        /// </summary>
        [DataField]
        public bool RequireBibleUser = true;
    }
