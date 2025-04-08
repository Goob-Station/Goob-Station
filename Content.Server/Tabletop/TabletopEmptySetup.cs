// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 eclips_e <67359748+Just-a-Unity-Dev@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using JetBrains.Annotations;

namespace Content.Server.Tabletop
{
    [UsedImplicitly]
    public sealed partial class TabletopEmptySetup : TabletopSetup
    {
        public override void SetupTabletop(TabletopSession session, IEntityManager entityManager)
        {
            var board = entityManager.SpawnEntity(BoardPrototype, session.Position.Offset(0, 0));
            session.Entities.Add(board);
        }
    }
}