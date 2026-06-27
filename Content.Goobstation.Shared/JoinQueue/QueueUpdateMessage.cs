using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.JoinQueue;

/// <summary>
///     Sent from server to client with queue state for player
///     Also initiates queue state on client
/// </summary>
public sealed class QueueUpdateMessage : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Command;

    // Queue info
    public int Total { get; set; }
    public int Position { get; set; }
    public bool IsPatron { get; set; }

    // Estimated wait
    public float EstimatedWaitSeconds { get; set; } = -1f;

    // Server info
    public string MapName { get; set; } = string.Empty;
    public string GameMode { get; set; } = string.Empty;
    public int ServerPlayerCount { get; set; }
    public int MaxPlayerCount { get; set; }
    public int RoundDurationMinutes { get; set; }

    // Critter display
    public string YourName { get; set; } = string.Empty;
    public List<string> PlayerNames { get; set; } = new();

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        Total = buffer.ReadInt32();
        Position = buffer.ReadInt32();
        IsPatron = buffer.ReadBoolean();
        EstimatedWaitSeconds = buffer.ReadFloat();

        MapName = buffer.ReadString();
        GameMode = buffer.ReadString();
        ServerPlayerCount = buffer.ReadInt32();
        MaxPlayerCount = buffer.ReadInt32();
        RoundDurationMinutes = buffer.ReadInt32();

        YourName = buffer.ReadString();
        var count = buffer.ReadInt32();
        PlayerNames = new List<string>(count);
        for (var i = 0; i < count; i++)
        {
            PlayerNames.Add(buffer.ReadString());
        }
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(Total);
        buffer.Write(Position);
        buffer.Write(IsPatron);
        buffer.Write(EstimatedWaitSeconds);

        buffer.Write(MapName);
        buffer.Write(GameMode);
        buffer.Write(ServerPlayerCount);
        buffer.Write(MaxPlayerCount);
        buffer.Write(RoundDurationMinutes);

        buffer.Write(YourName);
        buffer.Write(PlayerNames.Count);
        foreach (var name in PlayerNames)
        {
            buffer.Write(name);
        }
    }
}
