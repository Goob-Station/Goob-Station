using Content.Goobstation.Shared.Animations;
using Content.Goobstation.Shared.Animations.API;

namespace Content.Goobstation.Server.Animations;

public sealed class AnimationPlayerSystem : EntitySystem
{
    public void PlayAnimation(EntityUid entityUid, AnimationPrototype animationPrototype) => PlayAnimation(entityUid, animationPrototype.ID);
    public void PlayAnimation(EntityUid entityUid, string animationPrototype) =>
        RaiseNetworkEvent(new PlayAnimationMessage(GetNetEntity(entityUid), animationPrototype));
}
