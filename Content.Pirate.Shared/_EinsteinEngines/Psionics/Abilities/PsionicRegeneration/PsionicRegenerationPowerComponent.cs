using Robust.Shared.Audio;
using Content.Shared.DoAfter;

namespace Content.Shared.Abilities.Psionics
{
    [RegisterComponent]
    public sealed partial class PsionicRegenerationPowerComponent : Component
    {
        [DataField]
        public DoAfterId? DoAfter;

        [DataField]
        public float EssenceAmount = 20;

        [DataField]
        public float UseDelay = 8f;
        [DataField]

        public SoundSpecifier SoundUse = new SoundPathSpecifier("/Audio/_EinsteinEngines/Psionics/heartbeat_fast.ogg");
    }
}

