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
