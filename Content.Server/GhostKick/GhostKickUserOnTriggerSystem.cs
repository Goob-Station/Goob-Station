// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 LordCarve <27449516+LordCarve@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Explosion.EntitySystems;
using Robust.Shared.Player;

namespace Content.Server.GhostKick;

public sealed class GhostKickUserOnTriggerSystem : EntitySystem
{
    [Dependency] private readonly GhostKickManager _ghostKickManager = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<GhostKickUserOnTriggerComponent, TriggerEvent>(HandleMineTriggered);
    }

    private void HandleMineTriggered(EntityUid uid, GhostKickUserOnTriggerComponent userOnTriggerComponent, TriggerEvent args)
    {
        if (!TryComp(args.User, out ActorComponent? actor))
            return;

        _ghostKickManager.DoDisconnect(
            actor.PlayerSession.Channel,
            "Tripped over a kick mine, crashed through the fourth wall");

        args.Handled = true;
    }
}