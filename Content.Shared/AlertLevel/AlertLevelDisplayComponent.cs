// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Shared.AlertLevel;

[RegisterComponent]
public sealed partial class AlertLevelDisplayComponent : Component
{
    [DataField("alertVisuals")]
    public  Dictionary<string, string> AlertVisuals = new();
}
