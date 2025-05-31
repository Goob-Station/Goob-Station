using Content.Casino.Shared.Data;
using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;
using Content.Casino.Shared.Games;

namespace Content.Casino.Shared.Network;

/// <summary>
/// Request to start a new game session.
/// </summary>
public sealed class StartGameRequest : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.EntityEvent;

    public string GameId { get; set; } = string.Empty;
    public int InitialBet { get; set; }

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        GameId = buffer.ReadString();
        InitialBet = buffer.ReadInt32();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(GameId);
        buffer.Write(InitialBet);
    }
}

/// <summary>
/// Response when a game session starts.
/// </summary>
public sealed class GameStartedMessage : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.EntityEvent;

    public string SessionId { get; set; } = string.Empty;
    public string GameId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string? SerializedGameState { get; set; }
    public GameAction[] AvailableActions { get; set; } = Array.Empty<GameAction>();

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        SessionId = buffer.ReadString();
        GameId = buffer.ReadString();
        Success = buffer.ReadBoolean();
        ErrorMessage = buffer.ReadString();
        SerializedGameState = buffer.ReadString();

        var actionCount = buffer.ReadInt32();
        AvailableActions = new GameAction[actionCount];
        for (int i = 0; i < actionCount; i++)
        {
            var actionId = buffer.ReadString();
            var displayName = buffer.ReadString();
            var parameters = buffer.ReadString(); // JSON serialized
            AvailableActions[i] = new GameAction(actionId, displayName, parameters);
        }
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(SessionId);
        buffer.Write(GameId);
        buffer.Write(Success);
        buffer.Write(ErrorMessage);
        buffer.Write(SerializedGameState ?? string.Empty);

        buffer.Write(AvailableActions.Length);
        foreach (var action in AvailableActions)
        {
            buffer.Write(action.ActionId);
            buffer.Write(action.DisplayName);
            buffer.Write(action.Parameters?.ToString() ?? string.Empty);
        }
    }
}

/// <summary>
/// Request to execute an action in a game session.
/// </summary>
public sealed class ExecuteActionRequest : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.EntityEvent;

    public string SessionId { get; set; } = string.Empty;
    public GameAction Action { get; set; }

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        SessionId = buffer.ReadString();
        var actionId = buffer.ReadString();
        var displayName = buffer.ReadString();
        var parameters = buffer.ReadString();
        Action = new GameAction(actionId, displayName, parameters);
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(SessionId);
        buffer.Write(Action.ActionId);
        buffer.Write(Action.DisplayName);
        buffer.Write(Action.Parameters?.ToString() ?? string.Empty);
    }
}

/// <summary>
/// Response when an action is executed.
/// </summary>
public sealed class ActionResultMessage : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.EntityEvent;

    public string SessionId { get; set; } = string.Empty;
    public GameAction Action { get; set; }
    public GameActionResult Result { get; set; }
    public GameAction[] UpdatedActions { get; set; } = Array.Empty<GameAction>();

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        SessionId = buffer.ReadString();

        // Read action
        var actionId = buffer.ReadString();
        var displayName = buffer.ReadString();
        var parameters = buffer.ReadString();
        Action = new GameAction(actionId, displayName, parameters);

        // Read result
        var isComplete = buffer.ReadBoolean();
        var won = buffer.ReadBoolean();
        var payout = buffer.ReadInt32();
        var message = buffer.ReadString();
        var gameState = buffer.ReadString(); // JSON serialized
        Result = new GameActionResult(isComplete, won, payout, message, gameState);

        // Read updated actions
        var actionCount = buffer.ReadInt32();
        UpdatedActions = new GameAction[actionCount];
        for (int i = 0; i < actionCount; i++)
        {
            var updatedActionId = buffer.ReadString();
            var updatedDisplayName = buffer.ReadString();
            var updatedParameters = buffer.ReadString();
            UpdatedActions[i] = new GameAction(updatedActionId, updatedDisplayName, updatedParameters);
        }
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(SessionId);

        // Write action
        buffer.Write(Action.ActionId);
        buffer.Write(Action.DisplayName);
        buffer.Write(Action.Parameters?.ToString() ?? string.Empty);

        // Write result
        buffer.Write(Result.IsComplete);
        buffer.Write(Result.Won);
        buffer.Write(Result.Payout);
        buffer.Write(Result.Message);
        buffer.Write(Result.GameState?.ToString() ?? string.Empty);

        // Write updated actions
        buffer.Write(UpdatedActions.Length);
        foreach (var action in UpdatedActions)
        {
            buffer.Write(action.ActionId);
            buffer.Write(action.DisplayName);
            buffer.Write(action.Parameters?.ToString() ?? string.Empty);
        }
    }
}

/// <summary>
/// Request to end a game session.
/// </summary>
public sealed class EndGameRequest : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.EntityEvent;

    public string SessionId { get; set; } = string.Empty;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        SessionId = buffer.ReadString();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(SessionId);
    }
}

/// <summary>
/// Response when a game session ends.
/// </summary>
public sealed class GameEndedMessage : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.EntityEvent;

    public string SessionId { get; set; } = string.Empty;
    public GameActionResult FinalResult { get; set; }

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        SessionId = buffer.ReadString();

        var isComplete = buffer.ReadBoolean();
        var won = buffer.ReadBoolean();
        var payout = buffer.ReadInt32();
        var message = buffer.ReadString();
        var gameState = buffer.ReadString();
        FinalResult = new GameActionResult(isComplete, won, payout, message, gameState);
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(SessionId);
        buffer.Write(FinalResult.IsComplete);
        buffer.Write(FinalResult.Won);
        buffer.Write(FinalResult.Payout);
        buffer.Write(FinalResult.Message);
        buffer.Write(FinalResult.GameState?.ToString() ?? string.Empty);
    }
}
