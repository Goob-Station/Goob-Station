// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 AJCM <AJCM@tutanota.com>
// SPDX-FileCopyrightText: 2024 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Shared.Magic.Events;

public sealed partial class InstantSpawnSpellEvent : InstantActionEvent
{
    /// <summary>
    /// What entity should be spawned.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId Prototype;

    [DataField]
    public bool PreventCollideWithCaster = true;

    /// <summary>
    /// Gets the targeted spawn positons; may lead to multiple entities being spawned.
    /// </summary>
    [DataField]
    public MagicInstantSpawnData PosData = new TargetCasterPos();
}
