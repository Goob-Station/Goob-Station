// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityEffects
{
    public sealed partial class PlaySoundEffect : EntityEffect
    {
        [DataField(required: true)]
        public SoundSpecifier Sound;

        protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
            => null;

        public override void Effect(EntityEffectBaseArgs args)
        {
            var transform = args.EntityManager.GetComponent<TransformComponent>(args.TargetEntity);
            var audioSys = args.EntityManager.EntitySysManager.GetEntitySystem<SharedAudioSystem>();

            audioSys.PlayPredicted(Sound, transform.Coordinates, args.TargetEntity);
        }
    }
}
