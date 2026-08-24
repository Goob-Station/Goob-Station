// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Goobstation.Wizard.Accents;

[RegisterComponent]
public sealed partial class JackalAccentComponent : AnimalAccentComponent
{
    public override List<LocId> AnimalNoises => new()
    {
        "accent-words-jackal-1",
        "accent-words-jackal-2",
        "accent-words-jackal-3",
        "accent-words-jackal-4",
    };
}