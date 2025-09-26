// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Dataset;
using Robust.Shared.Prototypes;

namespace Content.Server.Damage.Components;

/// <summary>
/// This component shows entity damage severity when it is examined by player.
/// </summary>
[RegisterComponent]
public sealed partial class ExaminableDamageComponent : Component
{
    /// <summary>
    /// ID of the <see cref="LocalizedDatasetPrototype"/> containing messages to display a different damage levels.
    /// The first message will be used at 0 damage with the others equally distributed across the range from undamaged to fully damaged.
    /// </summary>
    [DataField]
    public ProtoId<LocalizedDatasetPrototype>? Messages;
}
