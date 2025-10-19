// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 Skubman <ba.fallaria@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Client.Light.Components;

/// <summary>
/// Fades out the <see cref="SharedPointLightComponent"/> attached to this entity.
/// </summary>
[RegisterComponent]
public sealed partial class LightFadeComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("duration")]
    public float Duration = 0.5f;

    // <summary>
    //   EE Plasmamen Change: The duration of the fade-in effect before starting the fade out effect.
    // </summary>
    [DataField]
    public float RampUpDuration = 0f;
}
