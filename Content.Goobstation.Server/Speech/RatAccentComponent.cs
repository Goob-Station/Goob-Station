// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.Speech;

[RegisterComponent]
public sealed partial class RatAccentComponent : AnimalAccentComponent
{
    public override List<LocId> AnimalNoises => new()
    {
        "accent-words-rat-1",
        "accent-words-rat-2",
        "accent-words-rat-3",
    };
}