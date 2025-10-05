using Robust.Shared.Serialization;

namespace Content.Shared._CorvaxGoob.Animation.API;

[Serializable, NetSerializable]
public sealed class PlayAnimationMessage : EntityEventArgs
{
    public PlayAnimationMessage(NetEntity animatedEntity, string animationID)
    {
        this.AnimatedEntity = animatedEntity;
        this.AnimationID = animationID;
    }

    public NetEntity AnimatedEntity;

    public string AnimationID = "";
}
