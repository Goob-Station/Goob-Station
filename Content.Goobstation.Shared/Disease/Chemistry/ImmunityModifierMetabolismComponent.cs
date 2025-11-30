using Robust.Shared.GameStates;
using System;

namespace Content.Goobstation.Shared.Disease.Chemistry
{
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    public sealed partial class ImmunityModifierMetabolismComponent : Component
    {
        [AutoNetworkedField, ViewVariables]
        public float GainRateModifier { get; set; }

        [AutoNetworkedField, ViewVariables]
        public float StrengthModifier { get; set; }

        /// <summary>
        /// When the current modifier is expected to end.
        /// </summary>
        [AutoNetworkedField, ViewVariables]
        public TimeSpan ModifierTimer { get; set; } = TimeSpan.Zero;
    }
}

