// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Goobstation.Wizard.Accents;

public abstract partial class AnimalAccentComponent : Component
{
    [DataField]
    public virtual List<LocId> AnimalNoises { get; set; }

    [DataField]
    public virtual List<LocId> AnimalAltNoises { get; set; }

    [DataField]
    public virtual float AltNoiseProbability { get; set; }
}