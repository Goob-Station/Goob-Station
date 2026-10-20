// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Silicon.Components;

/// <summary>
/// Recharges the attached battery item by draining power from the cyborg chassis holding it.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BorgBatteryDrainRechargeComponent : Component
{
    /// <summary>
    /// Rate at which the entity's battery recharges (in Joules/Watts per second).
    /// </summary>
    [DataField, AutoNetworkedField]
    public float RechargeRate = 40f;

    /// <summary>
    /// Multiplier for how much power is drained from the cyborg chassis's battery per point of charge restored.
    /// E.g. 2.0 means 2 Joules drained from the cyborg's power cell per 1 Joule restored to the weapon.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float DrainMultiplier = 2.0f;
}
