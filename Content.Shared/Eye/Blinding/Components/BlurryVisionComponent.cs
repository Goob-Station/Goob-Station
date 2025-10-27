// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nairod <110078045+Nairodian@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deathride58 <deathride58@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Eye.Blinding.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Eye.Blinding.Components;

/// <summary>
///     This component adds a white overlay to the viewport. It does not actually cause blurring.
/// </summary>
[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(BlurryVisionSystem))]
public sealed partial class BlurryVisionComponent : Component
{
    /// <summary>
    ///     Amount of "blurring". Also modifies examine ranges.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("magnitude"), AutoNetworkedField]
    public float Magnitude = 4f; // Goobstation

    /// <summary>
    ///     Exponent that controls the magnitude of the effect.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("correctionPower"), AutoNetworkedField]
    public float CorrectionPower = 2f; // Goobstation

    public const float MaxMagnitude = 6;
    public const float DefaultCorrectionPower = 2f;
}