using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Lobby;

[Serializable, NetSerializable]
public sealed class LobbyRoundInfoEvent(int roundId, string mapName, string modeTitle, int playerCount, int readyCount) : EntityEventArgs
{
    public readonly int RoundId = roundId;
    public readonly string MapName = mapName;
    public readonly string ModeTitle = modeTitle;
    public readonly int PlayerCount = playerCount;
    public readonly int ReadyCount = readyCount;
}
