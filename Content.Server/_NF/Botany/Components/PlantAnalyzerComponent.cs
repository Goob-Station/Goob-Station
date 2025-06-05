// SPDX-FileCopyrightText: 2024 Dvir <39403717+dvir001@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DoAfter;
using Robust.Shared.Audio;

namespace Content.Server._NF.Botany.Components;

/// <summary>
///    After scanning, retrieves the target Uid to use with its related UI.
/// </summary>
[RegisterComponent]
public sealed partial class PlantAnalyzerComponent : Component
{
    [DataDefinition]
    public partial struct PlantAnalyzerSetting
    {
        [DataField]
        public bool AdvancedScan;

        [DataField]
        public float ScanDelay;

        [DataField]
        public float AdvScanDelay;
    }

    [DataField, ViewVariables]
    public PlantAnalyzerSetting Settings;

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public DoAfterId? DoAfter;

    [DataField]
    public SoundSpecifier? ScanningEndSound;
}
