using Content.Pirate.Shared.Mage.Components;
using Content.Shared.Alert;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Popups;

namespace Content.Pirate.Server.Mage.EntitySystems;

public sealed class MageManaSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private readonly Dictionary<ManaThreshold, string> _powerDictionary;

    public MageManaSystem()
    {
        var Locale = IoCManager.Resolve<ILocalizationManager>(); // Whyyyy

        _powerDictionary = new Dictionary<ManaThreshold, string>
        {
            { ManaThreshold.Max, Locale.GetString("shadowkin-power-max") },
            { ManaThreshold.Great, Locale.GetString("shadowkin-power-great") },
            { ManaThreshold.Good, Locale.GetString("shadowkin-power-good") },
            { ManaThreshold.Okay, Locale.GetString("shadowkin-power-okay") },
            { ManaThreshold.Tired, Locale.GetString("shadowkin-power-tired") },
            { ManaThreshold.Min, Locale.GetString("shadowkin-power-min") }
        };
    }

    /// <param name="powerLevel">The current power level.</param>
    /// <returns>The name of the power level.</returns>
    public string GetLevelName(float powerLevel)
    {
        // Placeholders
        var result = ManaThreshold.Min;
        var value = MageComponent.ManaThresholds[ManaThreshold.Max];

        // Find the highest threshold that is lower than the current power level
        foreach (var threshold in MageComponent.ManaThresholds)
        {
            if (threshold.Value <= value &&
                threshold.Value >= powerLevel)
            {
                result = threshold.Key;
                value = threshold.Value;
            }
        }

        // Return the name of the threshold
        _powerDictionary.TryGetValue(result, out var powerType);
        powerType ??= Loc.GetString("shadowkin-power-okay");
        return powerType;
    }

    /// <summary>
    ///     Sets the alert level of a shadowkin.
    /// </summary>
    /// <param name="uid">The entity uid.</param>
    /// <param name="enabled">Enable the alert or not</param>
    /// <param name="powerLevel">The current power level.</param>
    public void UpdateAlert(EntityUid uid, bool enabled, float? powerLevel = null)
    {
        if (!_entity.TryGetComponent<MageComponent>(uid, out var component))
        {
            Logger.ErrorS("MageManaSystem", "Tried to update alert of entity without mage component.");
            return;
        }

        if (!enabled || powerLevel == null)
        {
            // _alerts.ClearAlert(uid, component.Alert);
            return;
        }

        // 250 / 7 ~= 35
        // Pwr / 35 ~= (0-7)
        // Round to ensure (0-7)
        var power = Math.Clamp(Math.Round(component.ManaLevel / 14), 0, 7);

        // Set the alert level
        // _alerts.ShowAlert(uid, component.Alert, (short) power);
    }


    /// <summary>
    ///     Tries to update the power level of a shadowkin based on an amount of seconds.
    /// </summary>
    /// <param name="uid">The entity uid.</param>
    /// <param name="frameTime">The time since the last update in seconds.</param>
    public bool TryUpdatePowerLevel(EntityUid uid, float frameTime)
    {
        // Check if the entity has a shadowkin component
        if (!_entity.TryGetComponent<MageComponent>(uid, out var component))
            return false;

        // Check if power gain is enabled
        if (!component.ManaLevelGainEnabled)
            return false;

        // Set the new power level
        UpdatePowerLevel(uid, frameTime);

        return true;
    }

    /// <summary>
    ///     Updates the power level of a shadowkin based on an amount of seconds.
    /// </summary>
    /// <param name="uid">The entity uid.</param>
    /// <param name="frameTime">The time since the last update in seconds.</param>
    public void UpdatePowerLevel(EntityUid uid, float frameTime)
    {
        // Get shadowkin component
        if (!_entity.TryGetComponent<MageComponent>(uid, out var component))
        {
            Logger.Error("Tried to update power level of entity without mage component.");
            return;
        }

        // Calculate new power level (P = P + t * G * M)
        var newPowerLevel =
            component.ManaLevel + frameTime * component.ManaLevelGain; //* component.PowerLevelGainMultiplier;

        // Clamp power level using clamp function
        newPowerLevel = Math.Clamp(newPowerLevel, component.ManaLevelMin, component.ManaLevelMax);

        // Set the new power level
        SetPowerLevel(uid, newPowerLevel);
    }


    /// <summary>
    ///     Tries to add to the power level of a shadowkin.
    /// </summary>
    /// <param name="uid">The entity uid.</param>
    /// <param name="amount">The amount to add to the power level.</param>
    public bool TryAddPowerLevel(EntityUid uid, float amount)
    {
        // Check if the entity has a shadowkin component
        if (!_entity.TryGetComponent<MageComponent>(uid, out _))
            return false;

        // Set the new power level
        AddPowerLevel(uid, amount);

        return true;
    }

    /// <summary>
    ///     Adds to the power level of a shadowkin.
    /// </summary>
    /// <param name="uid">The entity uid.</param>
    /// <param name="amount">The amount to add to the power level.</param>
    public void AddPowerLevel(EntityUid uid, float amount)
    {
        // Get shadowkin component
        if (!_entity.TryGetComponent<MageComponent>(uid, out var component))
        {
            Logger.Error("Tried to add to power level of entity without mage component.");
            return;
        }

        // Get new power level
        var newPowerLevel = component.ManaLevel + amount;

        // Clamp power level using clamp function
        newPowerLevel = Math.Clamp(newPowerLevel, component.ManaLevelMin, component.ManaLevelMax);

        // Set the new power level
        SetPowerLevel(uid, newPowerLevel);
    }


    /// <summary>
    ///     Sets the power level of a shadowkin.
    /// </summary>
    /// <param name="uid">The entity uid.</param>
    /// <param name="newPowerLevel">The new power level.</param>
    public void SetPowerLevel(EntityUid uid, float newPowerLevel)
    {
        // Get shadowkin component
        if (!_entity.TryGetComponent<MageComponent>(uid, out var component))
        {
            Logger.Error("Tried to set power level of entity without mage component.");
            return;
        }

        // Clamp power level using clamp function
        newPowerLevel = Math.Clamp(newPowerLevel, component.ManaLevelMin, component.ManaLevelMax);

        // Set the new power level
        component._manaLevel = newPowerLevel;
    }

    public bool TryUseAbility(EntityUid uid, MageComponent component, FixedPoint2 abilityCost)
    {
        if (component.ManaLevel <= abilityCost)
        {
            _popup.PopupEntity(Loc.GetString("mage-not-enough-mana"), uid, uid);
            return false;
        }

        TryAddPowerLevel(uid, -abilityCost.Float());

        return true;
    }
}
