using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;

namespace Content.Shared._Shitmed.Weapons.Melee.Events;

public sealed class AttemptHandsMeleeEvent(BodyPartSymmetry? targetBodyPartSymmetry = null) : CancellableEntityEventArgs, IBodyPartRelayEvent, IBoneRelayEvent
{
    public BodyPartType TargetBodyPart => BodyPartType.Hand;
    public BodyPartSymmetry? TargetBodyPartSymmetry => targetBodyPartSymmetry;

    public bool RaiseOnParent => true;

    public bool Handled { get; set; }

}
