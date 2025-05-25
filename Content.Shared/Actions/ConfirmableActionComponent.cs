// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BombasterDS <deniskaporoshok@gmail.com>
// SPDX-FileCopyrightText: 2025 BombasterDS2 <shvalovdenis.workmail@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Popups;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Actions;

/// <summary>
/// An action that must be confirmed before using it.
/// Using it for the first time primes it, after a delay you can then confirm it.
/// Used for dangerous actions that cannot be undone (unlike screaming).
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(ConfirmableActionSystem))]
[AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class ConfirmableActionComponent : Component
{
    /// <summary>
    /// Warning popup shown when priming the action. 
    /// </summary>
    // Goobstation - Modsuits - Removed required string
    [DataField]
    public string Popup = string.Empty;

    /// <summary>
    /// Type of warning popup - Goobstaiton - Modsuits
    /// </summary>
    [DataField("popupType")]
    public PopupType PopupFontType = PopupType.LargeCaution;

    /// <summary>
    /// If not null, this is when the action can be confirmed at.
    /// This is the time of priming plus the delay.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan? NextConfirm;

    /// <summary>
    /// If not null, this is when the action will unprime at.
    /// This is <c>NextConfirm> plus <c>PrimeTime</c>
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan? NextUnprime;

    /// <summary>
    /// Forced delay between priming and confirming to prevent accidents.
    /// </summary>
    [DataField]
    public TimeSpan ConfirmDelay = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Once you prime the action it will unprime after this length of time.
    /// </summary>
    [DataField]
    public TimeSpan PrimeTime = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Goobstation
    /// Whether this action should cancel itself to confirm or not
    /// </summary>
    [DataField]
    public bool ShouldCancel = true;
}