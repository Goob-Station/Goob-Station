// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Winkarst <74284083+Winkarst-cpu@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Bible.Components
{
    /// <summary>
    /// This lets you summon a mob or item with an alternative verb on the item
    /// </summary>
    [RegisterComponent]
    public sealed partial class SummonableComponent : Component
    {
        /// <summary>
        /// Default sound to play when entity is summoned.
        /// </summary>
        private static readonly ProtoId<SoundCollectionPrototype> DefaultSummonSound = new("Summon");

        /// <summary>
        /// Sound to play when entity is summoned.
        /// </summary>
        [DataField]
        public SoundSpecifier SummonSound = new SoundCollectionSpecifier(DefaultSummonSound, AudioParams.Default.WithVolume(-4f));

        /// <summary>
        /// Used for a special item only the Chaplain can summon. Usually a mob, but supports regular items too.
        /// </summary>
        [DataField("specialItem", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string? SpecialItemPrototype = null;
        public bool AlreadySummoned = false;

        [DataField("requiresBibleUser")]
        public bool RequiresBibleUser = true;

        /// <summary>
        /// The specific creature this summoned, if the SpecialItemPrototype has a mobstate.
        /// </summary>
        [ViewVariables]
        public EntityUid? Summon = null;

        [DataField("summonAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string SummonAction = "ActionBibleSummon";

        [DataField("summonActionEntity")]
        public EntityUid? SummonActionEntity;

        /// Used for respawning
        [DataField("accumulator")]
        public float Accumulator = 0f;
        [DataField("respawnTime")]
        public float RespawnTime = 180f;
    }
}