using Robust.Shared.Animations;
using Robust.Shared.Audio;

namespace Content.Shared._CorvaxGoob.Animation.API;

[Serializable, DataDefinition]
public abstract partial class AnimationTrackData
{
    [DataField]
    [AlwaysPushInheritance]
    public List<KeyFrameData> KeyFrames;
}

[Serializable, DataDefinition]
public sealed partial class AnimationTrackComponentPropertyData : AnimationTrackData
{
    [DataField]
    [AlwaysPushInheritance]
    public string ComponentType;

    [DataField]
    [AlwaysPushInheritance]
    public string Property;

    [DataField]
    [AlwaysPushInheritance]
    public AnimationInterpolationMode InterpolationMode;
}

[Serializable, DataDefinition]
public sealed partial class AnimationTrackPlaySoundData : AnimationTrackData
{

}

[Serializable, DataDefinition]
public abstract partial class KeyFrameData
{
    [DataField]
    [AlwaysPushInheritance]
    public float Keyframe;
}

[Serializable, DataDefinition]
public sealed partial class KeyFrameSoundData : KeyFrameData
{
    [DataField]
    [AlwaysPushInheritance]
    public SoundSpecifier Sound;
}

[Serializable, DataDefinition]
public sealed partial class KeyFrameComponentPropertyData : KeyFrameData
{
    [DataField]
    [AlwaysPushInheritance]
    public string Value;

    [DataField]
    [AlwaysPushInheritance]
    public string Type;
}
