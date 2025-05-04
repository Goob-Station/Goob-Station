// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Access;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Explosion.Components
{
    /// <summary>
    /// Sends a trigger when the keyphrase is heard. This one is ID locked.
    /// </summary>
    [RegisterComponent]
    public sealed partial class TriggerOnVoiceIdLockedComponent : Component
    {

        /// <summary>
        ///     The keyphrase that the trigger listens for.
        /// </summary>
        [DataField]
        public LocId KeyPhrase;

        /// <summary>
        ///     The range at which it listens for keywords.
        /// </summary>
        [DataField]
        public int ListenRange { get; private set; } = 2;

        [DataField]
        public TimeSpan ActivationCooldown = TimeSpan.FromSeconds(5);

        /// <summary>
        /// trigger only if the entity saying the phrase is the entity holding it
        /// </summary>
        [DataField]
        public bool HolderOnly;

        [ViewVariables]
        public TimeSpan NextActivationTime;

        [ViewVariables]
        public EntityUid? User;

    }
}
