// SPDX-FileCopyrightText: 2023 Bixkitts <72874643+Bixkitts@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

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
