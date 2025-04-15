// SPDX-FileCopyrightText: 2022 Steven K <84935671+ModeratelyAware@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Client.Botany.Components;

[RegisterComponent]
public sealed partial class PotencyVisualsComponent : Component
{
    [DataField("minimumScale")]
    public float MinimumScale = 1f;

    [DataField("maximumScale")]
    public float MaximumScale = 2f;
}