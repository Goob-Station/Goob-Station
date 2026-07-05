// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._EinsteinEngines.TelescopicBaton;

[RegisterComponent]
public sealed partial class TelescopicBatonComponent : Component
{
    [DataField]
    public bool CanDropItems;

    [DataField]
    public bool AlwaysDropItems;

    /// <summary>
    ///     The amount of time during which the baton will be able to knockdown someone after activating it.
    /// </summary>
    [DataField]
    public TimeSpan AttackTimeframe = TimeSpan.FromSeconds(1.8f);

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan TimeframeAccumulator = TimeSpan.Zero;
}
