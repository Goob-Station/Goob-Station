using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Heatlamp;

/// <summary>
///     Component that holds heatlamp state. Enabled state is stored on an ItemToggleComponent,
///     if an ItemToggleComponent is not present, it is treated as enabled at all times.
/// </summary>
/// <remarks>
///     HeatlampComponent is quite large, though most of the data on it very rarely updates. The
///     only hot values should be the time until the next update, which is not sent to the client
///     since the only thing it impacts is temperature updates.
/// </remarks>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class HeatlampComponent : Component
{
    #region Temperature regulation

    /// <summary>
    ///     Power required to heat the attached entity by one degree kelvin without upgrades.
    /// </summary>
    [DataField]
    public float BaseHeatingPowerDrain = 2f;

    /// <summary>
    ///     Value of BaseHeatingPowerDrain calculated with upgrades.
    /// </summary>
    [AutoNetworkedField]
    [ViewVariables]
    public float ModifiedHeatingPowerDrain = float.NaN; // Updated on component init

    /// <summary>
    ///     Maximum amount that an entity can be heated each update without upgrades.
    /// </summary>
    [DataField]
    public float BaseMaximumHeatingPerUpdate = 5f;

    /// <summary>
    ///     Value of BaseMaximumHeatingPerUpdate calculated with upgrades.
    /// </summary>
    [AutoNetworkedField]
    [ViewVariables]
    public float ModifiedMaximumHeatingPerUpdate = float.NaN;

    /// <summary>
    ///     Power required to cool the attached entity by one degree kelvin without upgrades.
    /// </summary>
    [DataField]
    public float BaseCoolingPowerDrain = 4f;

    /// <summary>
    ///     Value of BaseCoolingPowerDrain calculated with upgrades.
    /// </summary>
    [AutoNetworkedField]
    [ViewVariables]
    public float ModifiedCoolingPowerDrain = float.NaN;

    /// <summary>
    ///     Maximum amount that an entity can be cooled per update without upgrades.
    /// </summary>
    [DataField]
    public float BaseMaximumCoolingPerUpdate = 5f;

    /// <summary>
    ///     Value of BaseMaximumCoolingPerUpdate calculated with upgrades
    /// </summary>
    [AutoNetworkedField]
    [ViewVariables]
    public float ModifiedMaximumCoolingPerUpdate = float.NaN;

    #endregion

    #region Damage

    /// <summary>
    ///     Deactivated damage. While it can't be upgraded, we store this here because HeatlampSystem replaces
    ///     the logic in ItemToggleMeleeWeaponComponent so that it works with upgrades.
    /// </summary>
    [DataField]
    public DamageSpecifier DeactivatedDamage;

    /// <summary>
    ///     Base activated damage.
    /// </summary>
    /// <remarks>
    ///     While the base fields aren't networked because they're part of the heatlamp prototype, the
    ///     base activated damage needs to be sent between the server and the client because it can
    ///     be modified by the emag interaction.
    /// </remarks>
    [DataField, AutoNetworkedField]
    public DamageSpecifier BaseActivatedDamage;

    /// <summary>
    ///     Value of BaseActivatedDamage calculated with upgrades
    /// </summary>
    [AutoNetworkedField]
    [ViewVariables]
    public DamageSpecifier ModifiedActivatedDamage;

    /// <summary>
    ///     Damage added by being emagged
    /// </summary>
    [DataField]
    public DamageSpecifier EmagDamageBoost;

    #endregion

    #region Upgrades

    /// <summary>
    ///     Maximum number of upgrades that can be inserted into a heatlamp.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int MaximumUpgradeCount = 2;

    /// <summary>
    ///     Current number of upgrade slots in the lamp. Used internally by the server when adding item slots to the thing.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int CurrentUpgradeCount = 0;

    /// <summary>
    ///     Number of additional upgrade slots being emagged gives a lamp.
    /// </summary>
    [DataField]
    public int EmagUpgradeCountIncrease = 2;

    /// <summary>
    ///     Heatlamp can be emagged. Intended for use with the thermal regulator organ.
    /// </summary>
    [DataField]
    public bool AllowEmag = true;

    /// <summary>
    ///     Heatlamp is emagged and can use illegal upgrades.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool AllowIllegalUpgrades = false;

    #endregion

    #region Updates

    /// <summary>
    ///     Delay between updates.
    /// </summary>
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(1);

    /// <summary>
    ///     Time of the last update.
    /// </summary>
    [DataField]
    public TimeSpan LastUpdate = TimeSpan.MinValue;

    #endregion
}
