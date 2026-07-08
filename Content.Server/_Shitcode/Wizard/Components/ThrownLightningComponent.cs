// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Goobstation.Wizard.Components;

[RegisterComponent]
public sealed partial class ThrownLightningComponent : Component
{
    [DataField]
    public TimeSpan StunTime = TimeSpan.FromSeconds(6);

    [DataField]
    public LocId? Speech = "action-speech-spell-thrown-lightning";
}
