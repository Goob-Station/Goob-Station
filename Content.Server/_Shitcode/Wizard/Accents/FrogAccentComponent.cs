// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Goobstation.Wizard.Accents;

[RegisterComponent]
public sealed partial class FrogAccentComponent : AnimalAccentComponent
{
    public override List<LocId> AnimalNoises => new()
    {
        "accent-words-frog-1",
        "accent-words-frog-2",
        "accent-words-frog-3",
        "accent-words-frog-4",
    };

    public override List<LocId> AnimalAltNoises => new()
    {
        "accent-words-alt-frog-1",
        "accent-words-alt-frog-2",
        "accent-words-alt-frog-3",
        "accent-words-alt-frog-4",
        "accent-words-alt-frog-5",
        "accent-words-alt-frog-6",
        "accent-words-alt-frog-7",
    };

    public override float AltNoiseProbability => 0.05f;
}