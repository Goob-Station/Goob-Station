using Content.Server.Administration.Managers;
using Content.Server.EUI;
using Content.Shared._MACRO.StrangeMoods;
using Content.Shared._MACRO.StrangeMoods.Eui;
using Content.Shared.Administration;
using Content.Shared.Eui;
using Robust.Server.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._MACRO.StrangeMoods.Eui;

public sealed class StrangeMoodsInitEui(
    StrangeMoodsSystem strangeMoods,
    EntityManager entity,
    IPrototypeManager prototype,
    IRobustRandom random,
    IAdminManager admin,
    IPlayerManager player,
    EuiManager eui,
    EntityUid user) : BaseEui
{
    private readonly ISawmill _sawmill = Logger.GetSawmill("strange-moods-init-eui");

    private EntityUid _target;

    public override EuiStateBase GetNewState()
    {
        return new StrangeMoodsInitEuiState(entity.GetNetEntity(_target));
    }

    public void SetTarget(EntityUid target)
    {
        _target = target;

        StateDirty();
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is not StrangeMoodsInitAcceptMessage message)
            return;

        if (!IsAllowed())
            return;

        if (!player.TryGetSessionByEntity(user, out var session))
            return;

        var target = entity.GetEntity(message.Target);
        var comp = StrangeMoodsSystem.CreateComponent(message.Definition);
        var ui = new StrangeMoodsEui(strangeMoods, entity, random, admin);
        SharedMood? sharedMood = null;

        entity.AddComponent(target, comp);
        var newComp = entity.GetComponent<StrangeMoodsComponent>(target);
        strangeMoods.RefreshMoods((target, newComp));

        var moods = newComp.StrangeMood.Moods;

        if (prototype.TryIndex(message.Definition.SharedMoodPrototype, out var sharedProto))
            strangeMoods.TryGetSharedMood(sharedProto, out sharedMood);

        eui.OpenEui(ui, session);
        ui.UpdateMoods(target, moods, sharedMood);
    }

    private bool IsAllowed()
    {
        var adminData = admin.GetAdminData(Player);

        if (adminData != null && adminData.HasFlag(AdminFlags.Moderator))
            return true;

        _sawmill.Warning($"Player {Player.UserId} tried to open / use strange moods init UI without permission.");
        return false;
    }
}