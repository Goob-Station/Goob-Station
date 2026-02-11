using Content.Goobstation.Shared.Mimery;
using Content.Server.Chat.Systems;
using Content.Shared._Goobstation.Wizard;
using Content.Shared.Abilities.Mime;
using Content.Shared.Actions;
using Content.Shared.Chat;

namespace Content.Goobstation.Server.Mimery;

public sealed class AdvancedMimerySystem : SharedAdvancedMimerySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

    protected override bool ShootFingerGuns(Entity<MimePowersComponent> ent, ref FingerGunsActionEvent args)
    {
        if (!base.ShootFingerGuns(ent, ref args) ||
            !TryComp(args.Action.Owner, out FingerGunsActionComponent? actionComp))
            return false;

        _chat.TrySendInGameICMessage(ent,
            Loc.GetString("finger-guns-event-shoot"),
            InGameICChatType.Emote,
            ChatTransmitRange.Normal);

        actionComp.UsesLeft--;
        if (actionComp.UsesLeft > 0)
            _actions.SetUseDelay(args.Action.Owner, actionComp.FireDelay);
        else
        {
            _actions.SetUseDelay(args.Action.Owner, actionComp.UseDelay);
            actionComp.UsesLeft = actionComp.CastAmount;
            RaiseNetworkEvent(new StopTargetingEvent(), ent);
        }

        return true;
    }
}
