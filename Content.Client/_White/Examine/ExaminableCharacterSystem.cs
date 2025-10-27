// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._White.Examine;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Content.Shared.Chat;
using Robust.Shared.Utility;
using Content.Client.UserInterface.Systems.Chat;
namespace Content.Client._White.Examine;

public sealed class ExaminableCharacterSystem : EntitySystem
{
    [Dependency] private readonly IUserInterfaceManager _ui = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<ExaminableCharacterInfoMessage>(OnExamineRichInfoResponse);
    }

    private void OnExamineRichInfoResponse(ExaminableCharacterInfoMessage ev)
    {
        Logger.Info($"Received ExaminableCharacterInfoMessage with message: {ev.Message}");
        var chatMsg = new ChatMessage(ChatChannel.Emotes,
            ev.Message.ToString(),
            ev.Message.ToMarkup(),
            NetEntity.Invalid,
            null);

        // TODO: For now BaseTextureTag is broken with chat stack.
        // But also, for now i don't wanna use, so if i want use textures
        // then i should fix it, instead of ignoring stacks
        //chatMsg.IgnoreChatStack = true;

        _ui.GetUIController<ChatUIController>().ProcessChatMessage(chatMsg);
    }
}
