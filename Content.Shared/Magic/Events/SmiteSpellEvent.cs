// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 AJCM <AJCM@tutanota.com>
// SPDX-FileCopyrightText: 2024 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;

namespace Content.Shared.Magic.Events;

public sealed partial class SmiteSpellEvent : EntityTargetActionEvent, ISpeakSpell
{
    // TODO: Make part of gib method
    /// <summary>
    /// Should this smite delete all parts/mechanisms gibbed except for the brain?
    /// </summary>
    [DataField]
    public bool DeleteNonBrainParts = true;

    [DataField]
    public string? Speech { get; private set; }
}