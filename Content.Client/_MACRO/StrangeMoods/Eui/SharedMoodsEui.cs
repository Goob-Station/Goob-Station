using Content.Client.Eui;
using Content.Shared._MACRO.StrangeMoods;
using Content.Shared._MACRO.StrangeMoods.Eui;
using Content.Shared.Eui;

namespace Content.Client._MACRO.StrangeMoods.Eui;

public sealed class SharedMoodsEui : BaseEui
{
    private readonly SharedMoodsUi _sharedMoodsUi;

    public SharedMoodsEui()
    {
        _sharedMoodsUi = new SharedMoodsUi();
        _sharedMoodsUi.OnCreateShared += CreateShared;
        _sharedMoodsUi.OnSave += SaveMoods;
        _sharedMoodsUi.OnSharedSelected += GetSharedMood;
    }

    private void CreateShared()
    {
        SendMessage(new SharedMoodsInitStartMessage());
    }

    private void SaveMoods()
    {
        var newMoods = _sharedMoodsUi.GetMoods();
        var targetMood = _sharedMoodsUi.GetTargetMood();

        if (targetMood is not { } target)
            return;

        SendMessage(new SharedMoodsSaveMessage(target, newMoods));
        _sharedMoodsUi.SetMoods(newMoods);
    }

    private void GetSharedMood(SharedMood mood)
    {
        if (mood.UniqueId == null)
            return;

        SendMessage(new SharedMoodsRequestMessage(mood.UniqueId));
    }

    public override void Opened()
    {
        _sharedMoodsUi.OpenCentered();
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        switch (msg)
        {
            case SharedMoodsSendMessage sendData:
                {
                    if (sendData.Mood is not { } mood)
                        return;

                    _sharedMoodsUi.SetMoods(mood.Moods);
                    break;
                }
            case SharedMoodsInitValidMessage initData:
                {
                    _sharedMoodsUi.PopulateDropDown(initData.AllSharedMoods, initData.Mood);
                    break;
                }
        }
    }

    public override void HandleState(EuiStateBase state)
    {
        if (state is not SharedMoodsEuiState s)
            return;

        _sharedMoodsUi.PopulateDropDown(s.AllSharedMoods, s.MoodId);
    }
}