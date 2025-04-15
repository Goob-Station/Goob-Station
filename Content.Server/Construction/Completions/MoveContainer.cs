// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Linq;
using Content.Shared.Construction;
using JetBrains.Annotations;
using Robust.Server.Containers;
using Robust.Shared.Containers;

namespace Content.Server.Construction.Completions
{
    [UsedImplicitly]
    [DataDefinition]
    public sealed partial class MoveContainer : IGraphAction
    {
        [DataField("from")] public string? FromContainer { get; private set; }
        [DataField("to")] public string? ToContainer { get; private set; }

        public void PerformAction(EntityUid uid, EntityUid? userUid, IEntityManager entityManager)
        {
            if (string.IsNullOrEmpty(FromContainer) || string.IsNullOrEmpty(ToContainer))
                return;

            var containerSystem = entityManager.EntitySysManager.GetEntitySystem<ContainerSystem>();
            var containerManager = entityManager.EnsureComponent<ContainerManagerComponent>(uid);

            var from = containerSystem.EnsureContainer<Container>(uid, FromContainer, containerManager);
            var to = containerSystem.EnsureContainer<Container>(uid, ToContainer, containerManager);

            foreach (var contained in from.ContainedEntities.ToArray())
            {
                if (containerSystem.Remove(contained, from))
                    containerSystem.Insert(contained, to);
            }
        }
    }
}