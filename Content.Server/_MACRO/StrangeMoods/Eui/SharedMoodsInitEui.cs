using System.Text.RegularExpressions;
using Content.Server.EUI;
using Content.Shared._MACRO.StrangeMoods;
using Content.Shared._MACRO.StrangeMoods.Eui;
using Content.Shared.Eui;

namespace Content.Server._MACRO.StrangeMoods.Eui;

public sealed partial class SharedMoodsInitEui(StrangeMoodsSystem strangeMoods) : BaseEui
{
    [GeneratedRegex("^[A-Za-z]+$")]
    private static partial Regex ProtoIdRegex();

    public event Action<HashSet<SharedMood>, SharedMood>? OnValidMessage;

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is not SharedMoodsInitAcceptMessage message)
            return;

        if (!ProtoIdRegex().IsMatch(message.Name) ||
            strangeMoods.SharedMoodIdExists(message.Name))
        {
            SendMessage(new SharedMoodsInitErrorMessage());
            return;
        }

        var mood = new SharedMood { UniqueId = message.Name, Count = 0 };
        strangeMoods.NewSharedMoods(mood);

        var allMoods = strangeMoods.GetSharedMoods();
        SendMessage(new SharedMoodsInitValidMessage(allMoods, mood));
        OnValidMessage?.Invoke(allMoods, mood);
    }
}