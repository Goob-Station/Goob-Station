using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;

namespace Content.Shared._Shitmed.Weapons.Ranged.Events;

public sealed class AttemptHandsShootEvent(BodyPartSymmetry? targetBodyPartSymmetry = null) : HandledEntityEventArgs, IBodyPartRelayEvent, IBoneRelayEvent
{
    public BodyPartType TargetBodyPart => BodyPartType.Hand;
    public BodyPartSymmetry? TargetBodyPartSymmetry => targetBodyPartSymmetry;

    public bool RaiseOnParent => true;
}
