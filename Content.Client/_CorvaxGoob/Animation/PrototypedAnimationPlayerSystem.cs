using Content.Shared._CorvaxGoob.Animation;
using Content.Shared._CorvaxGoob.Animation.API;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client._CorvaxGoob.Animation;

public sealed class PrototypedAnimationPlayerSystem : EntitySystem
{
    [Dependency] private readonly IComponentFactory _component = default!;
    [Dependency] private readonly AnimationPlayerSystem _anim = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private const string AnimationKey = "prototyped-animation";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<PlayAnimationMessage>(OnPlayAnimationMessage);
    }

    private void OnPlayAnimationMessage(PlayAnimationMessage ev)
    {
        if (_prototype.TryIndex<AnimationPrototype>(ev.AnimationID, out var animation))
            PlayAnimation(GetEntity(ev.AnimatedEntity), animation);
    }

    /// <summary>
    /// Plays prototyped animation on given entity.
    /// </summary>
    public void PlayAnimation(EntityUid entityUid, AnimationPrototype animationPrototype)
    {
        if (!entityUid.Valid)
            return;

        if (_anim.HasRunningAnimation(entityUid, AnimationKey))
            return;

        var animation = new Robust.Client.Animations.Animation();

        foreach (var track in animationPrototype.Tracks)
        {
            if (track is AnimationTrackComponentPropertyData)
            {
                var instance = (AnimationTrackComponentPropertyData) track;
                var trackData = new AnimationTrackComponentProperty();

                _component.TryGetRegistration(instance.ComponentType, out var registration, true);
                if (registration is null)
                    return;

                var comp = _component.GetComponent(registration.Idx);

                trackData.ComponentType = comp.GetType();
                if (trackData.ComponentType is null)
                    continue;

                trackData.Property = instance.Property;
                trackData.InterpolationMode = instance.InterpolationMode;

                foreach (var keyFrame in track.KeyFrames)
                {
                    if (keyFrame is not KeyFrameComponentPropertyData)
                        continue;

                    var keyFrameInstance = (KeyFrameComponentPropertyData) keyFrame;

                    object result = keyFrameInstance.Type.ToLower() switch
                    {
                        "int" => int.Parse(keyFrameInstance.Value),
                        "float" => float.Parse(keyFrameInstance.Value),
                        "vector2" => YamlHelpers.AsVector2(keyFrameInstance.Value),
                        "angle" => Angle.FromDegrees(float.Parse(keyFrameInstance.Value)),
                        _ => 0
                    };

                    trackData.KeyFrames.Add(new AnimationTrackProperty.KeyFrame(result, keyFrame.Keyframe));
                }

                animation.AnimationTracks.Add(trackData);
            }
            else if (track is AnimationTrackPlaySoundData)
            {
                var instance = (AnimationTrackPlaySoundData) track;
                var trackData = new AnimationTrackPlaySound();

                foreach (var keyFrame in track.KeyFrames)
                {
                    if (keyFrame is not KeyFrameSoundData)
                        continue;

                    trackData.KeyFrames.Add(new AnimationTrackPlaySound.KeyFrame(_audio.ResolveSound(((KeyFrameSoundData) keyFrame).Sound), keyFrame.Keyframe));
                }

                animation.AnimationTracks.Add(trackData);
            }
        }
        animation.Length = TimeSpan.FromSeconds(animationPrototype.Length);

        _anim.Play(entityUid, animation, AnimationKey);
    }
}
