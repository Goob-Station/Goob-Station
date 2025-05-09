// SPDX-FileCopyrightText: 2020 Campbell Suter <znix@znix.xyz>
// SPDX-FileCopyrightText: 2020 ComicIronic <comicironic@gmail.com>
// SPDX-FileCopyrightText: 2020 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 silicons <2003111+silicons@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Atmos;

namespace Content.Server.Atmos
{
    public struct MonstermosInfo
    {
        [ViewVariables]
        public int LastCycle;

        [ViewVariables]
        public long LastQueueCycle;

        [ViewVariables]
        public long LastSlowQueueCycle;

        [ViewVariables]
        public float MoleDelta;

        [ViewVariables]
        public float TransferDirectionEast;

        [ViewVariables]
        public float TransferDirectionWest;

        [ViewVariables]
        public float TransferDirectionNorth;

        [ViewVariables]
        public float TransferDirectionSouth;

        [ViewVariables]
        public float CurrentTransferAmount;

        [ViewVariables]
        public AtmosDirection CurrentTransferDirection;

        [ViewVariables]
        public bool FastDone;

        public float this[AtmosDirection direction]
        {
            get =>
                direction switch
                {
                    AtmosDirection.East => TransferDirectionEast,
                    AtmosDirection.West => TransferDirectionWest,
                    AtmosDirection.North => TransferDirectionNorth,
                    AtmosDirection.South => TransferDirectionSouth,
                    _ => throw new ArgumentOutOfRangeException(nameof(direction))
                };

            set
            {
                switch (direction)
                {
                    case AtmosDirection.East:
                         TransferDirectionEast = value;
                         break;
                    case AtmosDirection.West:
                        TransferDirectionWest = value;
                        break;
                    case AtmosDirection.North:
                        TransferDirectionNorth = value;
                        break;
                    case AtmosDirection.South:
                        TransferDirectionSouth = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(direction));
                }
            }
        }

        public float this[int index]
        {
            get => this[(AtmosDirection) (1 << index)];
            set => this[(AtmosDirection) (1 << index)] = value;
        }
    }
}