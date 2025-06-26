// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 osjarw <62134478+osjarw@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Xenoarchaeology.Equipment;
using Content.Shared.Xenoarchaeology.Equipment.Components;

namespace Content.Server.Xenoarchaeology.Equipment.Systems;

/// <inheritdoc cref="SharedNodeScannerSystem"/>
public sealed class NodeScannerSystem : SharedNodeScannerSystem
{
    protected override void TryOpenUi(Entity<NodeScannerComponent> device, EntityUid actor)
    {
        // no-op
    }
}