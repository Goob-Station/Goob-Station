// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Storage;
using Content.Shared.Storage.Components;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Whitelist;
using JetBrains.Annotations;
using Robust.Shared.Containers;

namespace Content.Server.Storage.EntitySystems
{
    [UsedImplicitly]
    public sealed class ItemCounterSystem : SharedItemCounterSystem
    {
        [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;
        protected override int? GetCount(ContainerModifiedMessage msg, ItemCounterComponent itemCounter)
        {
            if (!EntityManager.TryGetComponent(msg.Container.Owner, out StorageComponent? component))
            {
                return null;
            }

            var count = 0;
            foreach (var entity in component.Container.ContainedEntities)
            {
                if (_whitelistSystem.IsWhitelistPass(itemCounter.Count, entity))
                    count++;
            }

            return count;
        }
    }
}