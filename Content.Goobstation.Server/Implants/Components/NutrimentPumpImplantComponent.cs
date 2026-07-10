// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.Implants.Components;

[RegisterComponent]
public sealed partial class NutrimentPumpImplantComponent : Component
{
    /// <summary>
    /// Amount to modify hunger by.
    /// </summary>
    [DataField]
    public float FoodRate = 15f;

    /// <summary>
    /// Amount to modify thirst by.
    /// </summary>
    [DataField]
    public float DrinkRate = 40f;

    /// <summary>
    /// Next execution time. (Explanatory, I know.)
    /// </summary>
    [DataField]
    public TimeSpan NextExecutionTime = TimeSpan.Zero;

    /// <summary>
    /// The time between each execution.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public TimeSpan ExecutionInterval = TimeSpan.FromSeconds(1);
}
