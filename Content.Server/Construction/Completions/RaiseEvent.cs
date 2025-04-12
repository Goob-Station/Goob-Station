// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Construction;
using JetBrains.Annotations;

namespace Content.Server.Construction.Completions
{
    [UsedImplicitly]
    public sealed partial class RaiseEvent : IGraphAction
    {
        [DataField("event", required:true)]
        public EntityEventArgs? Event { get; private set; }

        [DataField("directed")]
        public bool Directed { get; private set; } = true;

        [DataField("broadcast")]
        public bool Broadcast { get; private set; } = true;

        public void PerformAction(EntityUid uid, EntityUid? userUid, IEntityManager entityManager)
        {
            if (Event == null || !Directed && !Broadcast)
                return;

            if(Directed)
                entityManager.EventBus.RaiseLocalEvent(uid, (object)Event);

            if(Broadcast)
                entityManager.EventBus.RaiseEvent(EventSource.Local, (object)Event);
        }
    }
}