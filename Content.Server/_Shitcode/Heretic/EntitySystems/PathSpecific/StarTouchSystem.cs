// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Chat.Systems;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Systems;
using Content.Shared.Actions;

namespace Content.Server._Shitcode.Heretic.EntitySystems.PathSpecific;

public sealed class StarTouchSystem : SharedStarTouchSystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

    public override void InvokeSpell(Entity<StarTouchComponent> ent, EntityUid user, bool deleteSpell = true)
    {
        base.InvokeSpell(ent, user, deleteSpell);

        _chat.TrySendInGameICMessage(user, Loc.GetString(ent.Comp.Speech), InGameICChatType.Speak, false);

        if (!deleteSpell)
            return;

        if (Exists(ent.Comp.StarTouchAction))
            _actions.SetCooldown(ent.Comp.StarTouchAction.Value, ent.Comp.Cooldown);

        QueueDel(ent);
    }
}
