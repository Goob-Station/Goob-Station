// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Pirate.Radio.Components;

[RegisterComponent]
public sealed partial class HandheldRadioFrequencyComponent : Component
{
    [DataField]
    public int Frequency;
}
