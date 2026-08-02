// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Goobstation.Wizard.Accents;

[RegisterComponent]
public sealed partial class CowAccentComponent : AnimalAccentComponent
{
    public override List<LocId> AnimalNoises => new()
    {
        "accent-words-cow-1",
        "accent-words-cow-2",
        "accent-words-cow-3",
    };
}