namespace Content.Goobstation.Common.Slasher.Events;

/// <summary>
///     relay of ReactionEntityEvent because dupe subscription.
///     yeah its ass but i dont care enough to fucking fix springlock suits
///     intentionally named this way so i can fix it later
///     todo marty
/// </summary>
[ByRefEvent]
public readonly record struct ShitRelayEventFixMeReactionEntityEvent()
{
}
