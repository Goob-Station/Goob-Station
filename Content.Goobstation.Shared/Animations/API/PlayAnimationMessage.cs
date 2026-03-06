using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Animations.API;

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
