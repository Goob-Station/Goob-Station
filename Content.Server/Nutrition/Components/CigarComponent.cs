// SPDX-License-Identifier: MIT

using Content.Server.Nutrition.EntitySystems;

namespace Content.Server.Nutrition.Components
{
    /// <summary>
    ///     A disposable, single-use smokable.
    /// </summary>
    [RegisterComponent, Access(typeof(SmokingSystem))]
    public sealed partial class CigarComponent : Component
    {
        /// <summary>
        ///     Goob - If a cigar can be ignited without a lighter by activating it
        /// </summary>
        [DataField("selfIgniting"), ViewVariables(VVAccess.ReadWrite)]
        public bool SelfIgniting { get; set; } = false;
    }
}
