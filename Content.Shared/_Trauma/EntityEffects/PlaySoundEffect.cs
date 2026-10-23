using Content.Shared.EntityEffects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._Trauma.EntityEffects;

public sealed partial class PlaySoundEffect : EntityEffectBase<PlaySoundEffect>
{
    /// <summary>
    /// The sound to play
    /// </summary>
    [DataField(required: true)]
    public SoundSpecifier Sound = default!;

    /// <summary>
    /// Play the sound at the position instead of parented to the target entity.
    /// Useful if the entity is deleted after.
    /// </summary>
    [DataField]
    public bool Positional = true;
}

public sealed partial class PlaySoundEffectSystem : EntityEffectSystem<TransformComponent, PlaySoundEffect>
{
    [Dependency] private SharedAudioSystem _audio = default!;

    protected override void Effect(Entity<TransformComponent> ent, ref EntityEffectEvent<PlaySoundEffect> args)
    {
        var sound = args.Effect.Sound;
        var user = args.User ?? ent.Owner; // only predicted for debug effect stick etc where there is a clear user

        if (args.Effect.Positional)
            _audio.PlayPvs(sound, ent.Comp.Coordinates);
        else
            _audio.PlayPvs(sound, ent);
    }
}
