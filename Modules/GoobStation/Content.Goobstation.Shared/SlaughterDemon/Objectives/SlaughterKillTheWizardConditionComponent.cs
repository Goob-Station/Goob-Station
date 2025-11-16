// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.SlaughterDemon.Objectives;

[RegisterComponent]
public sealed partial class SlaughterKillTheWizardConditionComponent : Component
{
    [DataField]
    public string? Title;

    [DataField]
    public string? Description;
}
