using System.Threading;
using Robust.Shared.Audio;

namespace Content.Shared.Abilities.Psionics
{
    [RegisterComponent]
    public sealed partial class HeadCageComponent : Component
    {
        public CancellationTokenSource? CancelToken;
        public bool IsActive = false;

        [DataField("startBreakoutSound")]
        public SoundSpecifier StartBreakoutSound { get; set; } = new SoundPathSpecifier("/Audio/Items/Handcuffs/cuff_breakout_start.ogg");

        [DataField("startUncageSound")]
        public SoundSpecifier StartUncageSound { get; set; } = new SoundPathSpecifier("/Audio/Items/Handcuffs/cuff_takeoff_start.ogg");

        [DataField("endUncageSound")]
        public SoundSpecifier EndUncageSound { get; set; } = new SoundPathSpecifier("/Audio/Items/Handcuffs/cuff_takeoff_end.ogg");

        [DataField("startCageSound")]
        public SoundSpecifier StartCageSound { get; set; } = new SoundPathSpecifier("/Audio/Items/Handcuffs/cuff_start.ogg");

        [DataField("endCageSound")]
        public SoundSpecifier EndCageSound { get; set; } = new SoundPathSpecifier("/Audio/Items/Handcuffs/cuff_end.ogg");

    }
}
