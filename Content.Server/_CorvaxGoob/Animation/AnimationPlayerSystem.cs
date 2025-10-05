using Content.Shared._CorvaxGoob.Animation;
using Content.Shared._CorvaxGoob.Animation.API;

namespace Content.Server._CorvaxGoob.Animation;

public sealed class AnimationPlayerSystem : EntitySystem
{
    public void PlayAnimation(EntityUid entityUid, AnimationPrototype animationPrototype) => PlayAnimation(entityUid, animationPrototype.ID);
    public void PlayAnimation(EntityUid entityUid, string animationPrototype) =>
        RaiseNetworkEvent(new PlayAnimationMessage(GetNetEntity(entityUid), animationPrototype));
}
