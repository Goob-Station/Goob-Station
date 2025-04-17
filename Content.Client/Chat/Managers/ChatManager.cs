// SPDX-FileCopyrightText: 2019 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2019 Silver <Silvertorch5@gmail.com>
// SPDX-FileCopyrightText: 2019 tentekal <tanner.fink@outlook.com>
// SPDX-FileCopyrightText: 2020 Bright <nsmoak10@yahoo.com>
// SPDX-FileCopyrightText: 2020 Bright0 <55061890+Bright0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Clement-O <topy72.mine@gmail.com>
// SPDX-FileCopyrightText: 2020 Clément <clement.orlandini@gmail.com>
// SPDX-FileCopyrightText: 2020 Exp <theexp111@gmail.com>
// SPDX-FileCopyrightText: 2020 Hugo Laloge <hugo.laloge@gmail.com>
// SPDX-FileCopyrightText: 2020 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2020 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Swept <jamesurquhartwebb@gmail.com>
// SPDX-FileCopyrightText: 2020 zumorica <zddm@outlook.es>
// SPDX-FileCopyrightText: 2021 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Clyybber <darkmine956@gmail.com>
// SPDX-FileCopyrightText: 2021 E F R <602406+Efruit@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Leo <lzimann@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2021 chairbender <kwhipke1@gmail.com>
// SPDX-FileCopyrightText: 2021 ike709 <ike709@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 ike709 <sparebytes@protonmail.com>
// SPDX-FileCopyrightText: 2021 mirrorcult <notzombiedude@gmail.com>
// SPDX-FileCopyrightText: 2022 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <Jezithyr.@gmail.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <Jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <jmaster9999@gmail.com>
// SPDX-FileCopyrightText: 2022 Michael Phillips <1194692+MeltedPixel@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Morbo <exstrominer@gmail.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <wrexbe@protonmail.com>
// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Vasilis <vascreeper@yahoo.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Rinary <72972221+Rinary1@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Administration.Managers;
using Content.Client.Ghost;
using Content.Shared.Administration;
using Content.Shared.Chat;
using Robust.Client.Console;
using Robust.Shared.Utility;

namespace Content.Client.Chat.Managers;

internal sealed class ChatManager : IChatManager
{
    [Dependency] private readonly IClientConsoleHost _consoleHost = default!;
    [Dependency] private readonly IClientAdminManager _adminMgr = default!;
    [Dependency] private readonly IEntitySystemManager _systems = default!;

    private ISawmill _sawmill = default!;
    public event Action? PermissionsUpdated; //Nyano - Summary: need to be able to update perms for new psionics.

    public void Initialize()
    {
        _sawmill = Logger.GetSawmill("chat");
        _sawmill.Level = LogLevel.Info;
    }

    public void SendAdminAlert(string message)
    {
        // See server-side manager. This just exists for shared code.
    }

    public void SendAdminAlert(EntityUid player, string message)
    {
        // See server-side manager. This just exists for shared code.
    }

    public void SendMessage(string text, ChatSelectChannel channel)
    {
        var str = text.ToString();
        switch (channel)
        {
            case ChatSelectChannel.Console:
                // run locally
                _consoleHost.ExecuteCommand(text);
                break;

            case ChatSelectChannel.LOOC:
                _consoleHost.ExecuteCommand($"looc \"{CommandParsing.Escape(str)}\"");
                break;

            case ChatSelectChannel.OOC:
                _consoleHost.ExecuteCommand($"ooc \"{CommandParsing.Escape(str)}\"");
                break;

            case ChatSelectChannel.Admin:
                _consoleHost.ExecuteCommand($"asay \"{CommandParsing.Escape(str)}\"");
                break;

            case ChatSelectChannel.Emotes:
                _consoleHost.ExecuteCommand($"me \"{CommandParsing.Escape(str)}\"");
                break;

            case ChatSelectChannel.Dead:
                if (_systems.GetEntitySystemOrNull<GhostSystem>() is {IsGhost: true})
                    goto case ChatSelectChannel.Local;

                if (_adminMgr.HasFlag(AdminFlags.Admin))
                    _consoleHost.ExecuteCommand($"dsay \"{CommandParsing.Escape(str)}\"");
                else
                    _sawmill.Warning("Tried to speak on deadchat without being ghost or admin.");
                break;

            // TODO sepearate radio and say into separate commands.
            case ChatSelectChannel.Radio:
            case ChatSelectChannel.Local:
                _consoleHost.ExecuteCommand($"say \"{CommandParsing.Escape(str)}\"");
                break;

            case ChatSelectChannel.Whisper:
                _consoleHost.ExecuteCommand($"whisper \"{CommandParsing.Escape(str)}\"");
                break;

            //Nyano - Summary: sends the command for telepath communication.
            case ChatSelectChannel.Telepathic:
                _consoleHost.ExecuteCommand($"tsay \"{CommandParsing.Escape(str)}\"");
                break;
            // Goobstation - Starlight collective mind port
            case ChatSelectChannel.CollectiveMind:
                _consoleHost.ExecuteCommand($"cmsay \"{CommandParsing.Escape(str)}\"");
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(channel), channel, null);
        }
    }

    //Nyano - Summary: fires off the update permissions script.
    public void UpdatePermissions()
    {
        PermissionsUpdated?.Invoke();
    }
}