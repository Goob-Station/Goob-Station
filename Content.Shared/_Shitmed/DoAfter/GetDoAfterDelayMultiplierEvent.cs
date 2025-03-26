using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;

namespace Content.Shared._Shitmed.DoAfter;

public sealed class GetDoAfterDelayMultiplierEvent(float multiplier = 1f) : EntityEventArgs, IBodyPartRelayEvent
{
    public float Multiplier = multiplier;

    public BodyPartType TargetBodyPart => BodyPartType.Hand;
}
