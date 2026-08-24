// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Content.Shared.Chemistry.Reaction;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityEffects;

public sealed partial class PlaySoundEffectSystem : EntityEffectSystem<ReactiveComponent, PlaySoundEffect>
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    protected override void Effect(Entity<ReactiveComponent> entity, ref EntityEffectEvent<PlaySoundEffect> args)
    {
        _audio.PlayPredicted(
            args.Effect.Sound,
            Transform(entity.Owner).Coordinates,
            entity.Owner);
    }
}

public sealed partial class PlaySoundEffect : EntityEffectBase<PlaySoundEffect>
{
    [DataField(required: true)]
    public SoundSpecifier Sound;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;
}
