using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.SpellCard;

/// <summary>
/// Raised on some client to play a spell card animation.
/// </summary>
[ImplicitDataDefinitionForInheritors]
[Serializable, NetSerializable]
public sealed partial class SpellCardAnimationEvent : EntityEventArgs
{
    public SpellCardAnimationEvent(SpellCardAnimationData animationData)
    {
        AnimationData = animationData;
    }

    public SpellCardAnimationData AnimationData;
}
