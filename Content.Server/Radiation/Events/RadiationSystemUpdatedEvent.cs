// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Radiation.Systems;

namespace Content.Server.Radiation.Events;

/// <summary>
///     Raised when <see cref="RadiationSystem"/> updated all
///     radiation receivers and radiation sources.
/// </summary>
public record struct RadiationSystemUpdatedEvent;