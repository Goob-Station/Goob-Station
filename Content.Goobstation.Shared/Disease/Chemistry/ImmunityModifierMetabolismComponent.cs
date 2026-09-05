using Robust.Shared.GameStates;
using System;

namespace Content.Goobstation.Shared.Disease.Chemistry
{
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    public sealed partial class ImmunityModifierMetabolismComponent : Component
    {
        [DataField, AutoNetworkedField]
        public float GainRateModifier { get; set; }

        [DataField, AutoNetworkedField]
        public float StrengthModifier { get; set; }

        /// <summary>
        /// When the current modifier is expected to end.
        /// </summary>
        [AutoNetworkedField]
        public TimeSpan ModifierTimer { get; set; } = TimeSpan.Zero;
    }
}

