using Content.Server.Chat.Systems;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Systems;
using Content.Shared.Actions;
using Content.Shared.Chat;

namespace Content.Server._Shitcode.Heretic.EntitySystems.PathSpecific;

public sealed class StarTouchSystem : SharedStarTouchSystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

    public override void InvokeSpell(Entity<StarTouchComponent> ent, EntityUid user)
    {
        base.InvokeSpell(ent, user);

        _chat.TrySendInGameICMessage(user, Loc.GetString(ent.Comp.Speech), InGameICChatType.Speak, false);

        if (Exists(ent.Comp.StarTouchAction))
            _actions.SetCooldown(ent.Comp.StarTouchAction.Value, ent.Comp.Cooldown);

        QueueDel(ent);
    }
}
