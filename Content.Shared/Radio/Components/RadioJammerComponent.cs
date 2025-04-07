// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 beck-thompson <107373427+beck-thompson@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Robust.Shared.Serialization;
using Robust.Shared.GameStates;

namespace Content.Shared.Radio.Components;

/// <summary>
/// When activated (<see cref="ActiveRadioJammerComponent"/>) prevents from sending messages in range
/// Suit sensors will also stop working.
/// </summary>
[NetworkedComponent, RegisterComponent]
[AutoGenerateComponentState]
public sealed partial class RadioJammerComponent : Component
{
    [DataDefinition]
    public partial struct RadioJamSetting
    {
        /// <summary>
        /// Power usage per second when enabled.
        /// </summary>
        [DataField(required: true)]
        public float Wattage;

        /// <summary>
        /// Range of the jammer.
        /// </summary>
        [DataField(required: true)]
        public float Range;

        /// <summary>
        /// The message that is displayed when switched.
        /// to this setting.
        /// </summary>
        [DataField(required: true)]
        public LocId Message = string.Empty;

        /// <summary>
        /// Name of the setting.
        /// </summary>
        [DataField(required: true)]
        public LocId Name = string.Empty;
    }

    /// <summary>
    /// List of all the settings for the radio jammer.
    /// </summary>
    [DataField(required: true), ViewVariables(VVAccess.ReadOnly)]
    public RadioJamSetting[] Settings;

    /// <summary>
    /// Index of the currently selected setting.
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public int SelectedPowerLevel = 1;
}

[Serializable, NetSerializable]
public enum RadioJammerChargeLevel : byte
{
    Low,
    Medium,
    High
}

[Serializable, NetSerializable]
public enum RadioJammerLayers : byte
{
    LED
}

[Serializable, NetSerializable]
public enum RadioJammerVisuals : byte
{
    ChargeLevel,
    LEDOn
}