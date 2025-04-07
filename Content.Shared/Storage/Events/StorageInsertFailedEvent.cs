// SPDX-FileCopyrightText: 2024 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Hands.Components;

namespace Content.Shared.Storage.Events;

[ByRefEvent]
public record struct StorageInsertFailedEvent(Entity<StorageComponent?> Storage, Entity<HandsComponent?> Player);