// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.Clothing.Components;

[RegisterComponent, AutoGenerateComponentState]
public sealed partial class MadnessMaskComponent : Component
{
    public float UpdateAccumulator = 0f;

    [DataField, AutoNetworkedField]
    public float UpdateTimer = 1f;

    [DataField, AutoNetworkedField]
    public bool AffectWearer = true;

    [DataField, AutoNetworkedField]
    public float StaminaProb = 0.4f;

    [DataField, AutoNetworkedField]
    public float JitterProb = 0.4f;

    [DataField, AutoNetworkedField]
    public float RainbowProb = 0.25f;

    [DataField, AutoNetworkedField]
    public float StaminaDamage = 10f;

    [DataField, AutoNetworkedField]
    public TimeSpan RainbowDuration = TimeSpan.FromSeconds(10f);
}
