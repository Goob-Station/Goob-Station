// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Client.Smoking;

[RegisterComponent]
public sealed partial class BurnStateVisualsComponent : Component
{
    [DataField("burntIcon")]
    public string BurntIcon = "burnt-icon";
    [DataField("litIcon")]
    public string LitIcon = "lit-icon";
    [DataField("unlitIcon")]
    public string UnlitIcon = "icon";
}

