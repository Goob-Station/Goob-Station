// SPDX-FileCopyrightText: 2022 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 Saphire <lattice@saphi.re>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Singularity.EntitySystems;
using Content.Shared.Singularity.Components;

namespace Content.Client.Singularity.Systems;

/// <summary>
/// The client-side version of <see cref="SharedEventHorizonSystem"/>.
/// Primarily manages <see cref="EventHorizonComponent"/>s.
/// Exists to make relevant signal handlers (ie: <see cref="SharedEventHorizonSystem.OnPreventCollide"/>) work on the client.
/// </summary>
public sealed class EventHorizonSystem : SharedEventHorizonSystem
{}