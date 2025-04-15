// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Goobstation.Wizard.Accents;

[RegisterComponent]
public sealed partial class FoxAccentComponent : AnimalAccentComponent
{
    public override List<LocId> AnimalNoises => new()
    {
        "accent-words-fox-1",
        "accent-words-fox-2",
        "accent-words-fox-3",
        "accent-words-fox-4",
        "accent-words-fox-5",
    };
}