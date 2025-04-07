using Robust.Shared.GameObjects;

namespace Content.Goobstation.Common.Stunnable;

public sealed class GetClothingStunModifierEvent : EntityEventArgs
{
    public GetClothingStunModifierEvent(EntityUid target)
    {
        Target = target;
    }

    public EntityUid Target;
    public float Modifier;
}
