// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Body.Organ;
namespace Content.Shared._Shitmed.Body.Organ;

public readonly record struct OrganComponentsModifyEvent(EntityUid Body, bool Add);

[ByRefEvent]
public readonly record struct OrganEnableChangedEvent(bool Enabled);

[ByRefEvent]
public readonly record struct OrganEnabledEvent(Entity<OrganComponent> Organ);

[ByRefEvent]
public readonly record struct OrganDisabledEvent(Entity<OrganComponent> Organ);