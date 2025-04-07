// SPDX-FileCopyrightText: 2024 AJCM <AJCM@tutanota.com>
// SPDX-FileCopyrightText: 2024 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;

namespace Content.Shared.Magic.Events;

/// <summary>
/// Adds provided Charge to the held wand
/// </summary>
public sealed partial class ChargeSpellEvent : InstantActionEvent, ISpeakSpell
{
    [DataField(required: true)]
    public int Charge;

    [DataField]
    public string WandTag = "WizardWand";

    [DataField]
    public string? Speech { get; private set; }
}