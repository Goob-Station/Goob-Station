// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.Speech;

public abstract partial class AnimalAccentComponent : Component
{
    [DataField]
    public virtual List<LocId> AnimalNoises { get; set; }

    [DataField]
    public virtual List<LocId> AnimalAltNoises { get; set; }

    [DataField]
    public virtual float AltNoiseProbability { get; set; }
}