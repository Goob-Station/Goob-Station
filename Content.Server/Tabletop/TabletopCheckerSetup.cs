// SPDX-FileCopyrightText: 2022 Demetre Beroshvili <69920861+Capnsockless@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 /ʊniɹɑː/ <onoira@psiko.zone>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 eclips_e <67359748+Just-a-Unity-Dev@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Tabletop
{
    [UsedImplicitly]
    public sealed partial class TabletopCheckerSetup : TabletopSetup
    {

        [DataField("prototypePieceWhite", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string PrototypePieceWhite = default!;

        [DataField("prototypeCrownWhite", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string PrototypeCrownWhite = default!;

        [DataField("prototypePieceBlack", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string PrototypePieceBlack = default!;

        [DataField("prototypeCrownBlack", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string PrototypeCrownBlack = default!;

        public override void SetupTabletop(TabletopSession session, IEntityManager entityManager)
        {
            session.Entities.Add(
                entityManager.SpawnEntity(BoardPrototype, session.Position.Offset(-1, 0))
            );

            SpawnPieces(session, entityManager, session.Position.Offset(-4.5f, 3.5f));
        }

        private void SpawnPieces(TabletopSession session, IEntityManager entityManager, MapCoordinates left)
        {
            static float GetOffset(float offset) => offset * 1f /* separation */;

            Span<EntityUid> pieces = stackalloc EntityUid[42];
            var pieceIndex = 0;

            // Pieces
            for (var offsetY = 0; offsetY < 3; offsetY++)
            {
                var checker = offsetY % 2;

                for (var offsetX = 0; offsetX < 8; offsetX += 2)
                {
                    // Prevents an extra piece on the middle row
                    if (checker + offsetX > 8) continue;

                    pieces[pieceIndex] = entityManager.SpawnEntity(
                        PrototypePieceBlack,
                        left.Offset(GetOffset(offsetX + (1 - checker)), GetOffset(offsetY * -1))
                    );
                    pieces[pieceIndex] = entityManager.SpawnEntity(
                        PrototypePieceWhite,
                        left.Offset(GetOffset(offsetX + checker), GetOffset(offsetY - 7))
                    );
                    pieceIndex += 2;
                }
            }

            const int NumCrowns = 3;
            const float Overlap = 0.25f;
            const float xOffset = 9f / 32;
            const float xOffsetBlack = 9 + xOffset;
            const float xOffsetWhite = 8 + xOffset;

            // Crowns
            for (var i = 0; i < NumCrowns; i++)
            {
                var step = -(Overlap * i);
                pieces[pieceIndex] = entityManager.SpawnEntity(
                    PrototypeCrownBlack,
                    left.Offset(GetOffset(xOffsetBlack), GetOffset(step))
                );
                pieces[pieceIndex + 1] = entityManager.SpawnEntity(
                    PrototypeCrownWhite,
                    left.Offset(GetOffset(xOffsetWhite), GetOffset(step))
                );
                pieceIndex += 2;
            }

            // Spares
            for (var i = 0; i < 6; i++)
            {
                var step = -((Overlap * (NumCrowns + 2)) + (Overlap * i));
                pieces[pieceIndex] = entityManager.SpawnEntity(
                    PrototypePieceBlack,
                    left.Offset(GetOffset(xOffsetBlack), GetOffset(step))
                );
                pieces[pieceIndex] = entityManager.SpawnEntity(
                    PrototypePieceWhite,
                    left.Offset(GetOffset(xOffsetWhite), GetOffset(step))
                );
                pieceIndex += 2;
            }

            for (var i = 0; i < pieces.Length; i++)
            {
                session.Entities.Add(pieces[i]);
            }
        }
    }
}