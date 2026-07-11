using Content.Server.Administration.Managers;
using Content.Server.EUI;
using Content.Shared._MACRO.StrangeMoods.Eui;
using Content.Shared.Administration;
using Content.Shared.Eui;
using Robust.Shared.Player;

namespace Content.Server._MACRO.StrangeMoods.Eui;

public sealed class SharedMoodsEui(
    StrangeMoodsSystem strangeMoods,
    IAdminManager admin,
    EuiManager eui,
    ICommonSession user) : BaseEui
{
    private readonly ISawmill _sawmill = Logger.GetSawmill("strange-moods-eui");

    private SharedMoodsInitEui? _sharedUi;
    private string? _targetMood;

    public override EuiStateBase GetNewState()
    {
        var sharedMoods = strangeMoods.GetSharedMoods();
        var mood = _targetMood;
        _targetMood = null;
        return new SharedMoodsEuiState(sharedMoods, mood);
    }

    public void SetMood(string? mood)
    {
        _targetMood = mood;
        StateDirty();
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (!IsAllowed())
            return;

        switch (msg)
        {
            case SharedMoodsInitStartMessage:
            {
                _sharedUi = new SharedMoodsInitEui(strangeMoods);
                _sharedUi.OnValidMessage += (allSharedMoods, mood) => SendMessage(new SharedMoodsInitValidMessage(allSharedMoods, mood));
                eui.OpenEui(_sharedUi, user);
                break;
            }
            case SharedMoodsSaveMessage saveData:
            {
                strangeMoods.NewSharedMoods(saveData.Target, saveData.Moods, true);
                break;
            }
            case SharedMoodsRequestMessage requestData:
            {
                if (!strangeMoods.TryGetSharedMood(requestData.MoodId, out var mood))
                    return;

                SendMessage(new SharedMoodsSendMessage(mood));
                break;
            }
        }
    }

    private bool IsAllowed()
    {
        var adminData = admin.GetAdminData(Player);

        if (adminData != null && adminData.HasFlag(AdminFlags.Moderator))
            return true;

        _sawmill.Warning($"Player {Player.UserId} tried to open / use strange moods UI without permission.");
        return false;
    }
}