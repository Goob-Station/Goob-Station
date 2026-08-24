// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Hands.Components;

namespace Content.Shared.Storage.Events;

[ByRefEvent]
public record struct StorageInsertFailedEvent(Entity<StorageComponent?> Storage, Entity<HandsComponent?> Player);