// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Religion.Nullrod.Components;

    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    public sealed partial class NullrodComponent : Component
    {
        /// <summary>
        /// Whether non bible-users are able to use null rod
        /// </summary>
        [DataField]
        public bool UntrainedUseRestriction;

        /// <summary>
        /// How much damage is dealt when an untrained user uses it.
        /// </summary>
        [DataField("DamageOnUntrainedUse", required: true)]
        public DamageSpecifier DamageOnUntrainedUse = default!;

        /// <summary>
        /// Which pop-up string to use.
        /// </summary>
        [DataField("UntrainedUseString", required: true)]
        public string UntrainedUseString = default!;

        /// <summary>
        /// Which sound to play on untrained use.
        /// </summary>
        [DataField]
        public SoundSpecifier UntrainedUseSound = new SoundPathSpecifier("/Audio/Effects/hallelujah.ogg");

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
        ///     When attempting attack against the same entity multiple times,
        ///     don't spam popups every frame and instead have a cooldown.
        /// </summary>
        [DataField]
        public TimeSpan PopupCooldown = TimeSpan.FromSeconds(3.0);

        [DataField, AutoNetworkedField]
        public TimeSpan? NextPopupTime;

        /// <summary>
        /// The last entity attacked, used for popup purposes (avoid spam)
        /// </summary>
        [DataField]
        public EntityUid? LastAttackedEntity;

    }
