using Content.Shared.Emoting;
using Content.Shared.Silicons.Borgs.Components;

namespace Content.Goobstation.Shared.Emoting
{
    /// <summary>
    ///     Raised on the borg just before a borg flips. Wow.
    ///     Cost is percentage amount of battery to drain in order to flip.
    /// </summary>
    [ByRefEvent]
    public sealed class BorgFlippingEvent : EntityEventArgs
    {
        public EntityUid Borg;
        public BorgChassisComponent BorgChassis;
        public float Cost;
        public BeforeEmoteEvent BeforeEmote;

        public BorgFlippingEvent(EntityUid borg, BorgChassisComponent borgChassis, float cost,  BeforeEmoteEvent beforeEmote)
        {
            Borg = borg;
            BorgChassis = borgChassis;
            Cost = cost;
            BeforeEmote = beforeEmote;
        }
    }
}
