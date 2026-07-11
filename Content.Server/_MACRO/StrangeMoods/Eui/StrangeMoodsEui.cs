using System.Diagnostics.CodeAnalysis;
using Content.Server.Administration.Managers;
using Content.Server.EUI;
using Content.Shared._MACRO.StrangeMoods;
using Content.Shared._MACRO.StrangeMoods.Eui;
using Content.Shared.Administration;
using Content.Shared.Eui;
using Robust.Shared.Random;

namespace Content.Server._MACRO.StrangeMoods.Eui;

public sealed class StrangeMoodsEui(
    StrangeMoodsSystem strangeMoods,
    EntityManager entity,
    IRobustRandom random,
    IAdminManager admin) : BaseEui
{
    private readonly ISawmill _sawmill = Logger.GetSawmill("strange-moods-eui");

    private List<StrangeMood> _moods = [];
    private SharedMood? _sharedMood;
    private EntityUid _target;

    public override EuiStateBase GetNewState()
    {
        var sharedMoods = strangeMoods.GetSharedMoods();
        return new StrangeMoodsEuiState(sharedMoods, _moods, _sharedMood, entity.GetNetEntity(_target));
    }

    public void UpdateMoods(EntityUid uid, List<StrangeMood> moods, SharedMood? sharedMood)
    {
        if (!IsAllowed())
            return;

        _moods = moods;
        _sharedMood = sharedMood;
        _target = uid;

        StateDirty();
    }

    public void UpdateMoods(Entity<StrangeMoodsComponent> ent)
    {
        UpdateMoods(ent, ent.Comp.StrangeMood.Moods, ent.Comp.SharedMood);
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (!IsAllowed())
            return;

        switch (msg)
        {
            case StrangeMoodsSaveMessage saveData:
            {
                if (!HasStrangeMoods(saveData.Target, out var uid, out var comp))
                    return;

                strangeMoods.SetSharedMood((uid, comp), saveData.SharedMoodId);
                strangeMoods.SetMoods((uid, comp), saveData.Moods);
                break;
            }
            case StrangeMoodsGenerateRequestMessage requestGenerateData:
            {
                if (!HasStrangeMoods(requestGenerateData.Target, out var uid, out var comp))
                    return;

                var activeMoods = strangeMoods.GetActiveMoods((uid, comp));

                if (comp.StrangeMood.Datasets.Count <= 0 ||
                    !strangeMoods.TryPick(random.Pick(comp.StrangeMood.Datasets).Key, out var moodProto, activeMoods))
                    return;

                var newMood = strangeMoods.RollMood(moodProto);
                SendMessage(new StrangeMoodsGenerateSendMessage(newMood));
                break;
            }
            case StrangeMoodsSharedRequestMessage requestSharedData:
            {
                if (requestSharedData.MoodId is not { } id ||
                    !strangeMoods.TryGetSharedMood(id, out var mood))
                    return;

                SendMessage(new StrangeMoodsSharedSendMessage(mood));
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

    private bool HasStrangeMoods(NetEntity ent, out EntityUid uid, [NotNullWhen(true)] out StrangeMoodsComponent? comp)
    {
        uid = entity.GetEntity(ent);
        if (entity.TryGetComponent(uid, out comp))
            return true;

        _sawmill.Warning($"Entity {entity.ToPrettyString(uid)} does not have StrangeMoodsComponent!");
        return false;
    }
}