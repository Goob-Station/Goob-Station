// SPDX-FileCopyrightText: 2020 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2020 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Arcade
{
    public static class BlockGameMessages
    {
        [Serializable, NetSerializable]
        public sealed class BlockGamePlayerActionMessage : BoundUserInterfaceMessage
        {
            public readonly BlockGamePlayerAction PlayerAction;
            public BlockGamePlayerActionMessage(BlockGamePlayerAction playerAction)
            {
                PlayerAction = playerAction;
            }
        }

        [Serializable, NetSerializable]
        public sealed class BlockGameVisualUpdateMessage : BoundUserInterfaceMessage
        {
            public readonly BlockGameVisualType GameVisualType;
            public readonly BlockGameBlock[] Blocks;
            public BlockGameVisualUpdateMessage(BlockGameBlock[] blocks, BlockGameVisualType gameVisualType)
            {
                Blocks = blocks;
                GameVisualType = gameVisualType;
            }
        }

        public enum BlockGameVisualType
        {
            GameField,
            HoldBlock,
            NextBlock
        }

        [Serializable, NetSerializable]
        public sealed class BlockGameScoreUpdateMessage : BoundUserInterfaceMessage
        {
            public readonly int Points;
            public BlockGameScoreUpdateMessage(int points)
            {
                Points = points;
            }
        }

        [Serializable, NetSerializable]
        public sealed class BlockGameUserStatusMessage : BoundUserInterfaceMessage
        {
            public readonly bool IsPlayer;

            public BlockGameUserStatusMessage(bool isPlayer)
            {
                IsPlayer = isPlayer;
            }
        }

        [Serializable, NetSerializable, Virtual]
        public class BlockGameSetScreenMessage : BoundUserInterfaceMessage
        {
            public readonly BlockGameScreen Screen;
            public readonly bool IsStarted;
            public BlockGameSetScreenMessage(BlockGameScreen screen, bool isStarted = true)
            {
                Screen = screen;
                IsStarted = isStarted;
            }
        }

        [Serializable, NetSerializable]
        public sealed class BlockGameGameOverScreenMessage : BlockGameSetScreenMessage
        {
            public readonly int FinalScore;
            public readonly int? LocalPlacement;
            public readonly int? GlobalPlacement;
            public BlockGameGameOverScreenMessage(int finalScore, int? localPlacement, int? globalPlacement) : base(BlockGameScreen.Gameover)
            {
                FinalScore = finalScore;
                LocalPlacement = localPlacement;
                GlobalPlacement = globalPlacement;
            }
        }

        [Serializable, NetSerializable]
        public enum BlockGameScreen
        {
            Game,
            Pause,
            Gameover,
            Highscores
        }

        [Serializable, NetSerializable]
        public sealed class BlockGameHighScoreUpdateMessage : BoundUserInterfaceMessage
        {
            public List<HighScoreEntry> LocalHighscores;
            public List<HighScoreEntry> GlobalHighscores;

            public BlockGameHighScoreUpdateMessage(List<HighScoreEntry> localHighscores, List<HighScoreEntry> globalHighscores)
            {
                LocalHighscores = localHighscores;
                GlobalHighscores = globalHighscores;
            }
        }

        [Serializable, NetSerializable]
        public sealed class HighScoreEntry : IComparable
        {
            public string Name;
            public int Score;

            public HighScoreEntry(string name, int score)
            {
                Name = name;
                Score = score;
            }

            public int CompareTo(object? obj)
            {
                if (obj is not HighScoreEntry entry) return 0;
                return Score.CompareTo(entry.Score);
            }
        }

        [Serializable, NetSerializable]
        public sealed class BlockGameLevelUpdateMessage : BoundUserInterfaceMessage
        {
            public readonly int Level;
            public BlockGameLevelUpdateMessage(int level)
            {
                Level = level;
            }
        }
    }
}