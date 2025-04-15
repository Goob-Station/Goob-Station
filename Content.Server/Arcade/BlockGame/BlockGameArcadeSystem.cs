// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 12rabbits <53499656+12rabbits@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Alzore <140123969+Blackern5000@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 ArtisticRoomba <145879011+ArtisticRoomba@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Brandon Hu <103440971+Brandon-Huu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Dimastra <65184747+Dimastra@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Dimastra <dimastra@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Eoin Mcloughlin <helloworld@eoinrul.es>
// SPDX-FileCopyrightText: 2024 IProduceWidgets <107586145+IProduceWidgets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 JIPDawg <51352440+JIPDawg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 JIPDawg <JIPDawg93@gmail.com>
// SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2024 Moomoobeef <62638182+Moomoobeef@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 PopGamer46 <yt1popgamer@gmail.com>
// SPDX-FileCopyrightText: 2024 PursuitInAshes <pursuitinashes@gmail.com>
// SPDX-FileCopyrightText: 2024 QueerNB <176353696+QueerNB@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Saphire Lattice <lattice@saphi.re>
// SPDX-FileCopyrightText: 2024 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Simon <63975668+Simyon264@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Spessmann <156740760+Spessmann@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 Thomas <87614336+Aeshus@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Winkarst <74284083+Winkarst-cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 eoineoineoin <github@eoinrul.es>
// SPDX-FileCopyrightText: 2024 github-actions[bot] <41898282+github-actions[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 stellar-novas <stellar_novas@riseup.net>
// SPDX-FileCopyrightText: 2024 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.UserInterface;
using Content.Server.Advertise.EntitySystems;
using Content.Shared.Advertise.Components;
using Content.Shared.Arcade;
using Content.Shared.Power;
using Robust.Server.GameObjects;

namespace Content.Server.Arcade.BlockGame;

public sealed class BlockGameArcadeSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly SpeakOnUIClosedSystem _speakOnUIClosed = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlockGameArcadeComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<BlockGameArcadeComponent, AfterActivatableUIOpenEvent>(OnAfterUIOpen);
        SubscribeLocalEvent<BlockGameArcadeComponent, PowerChangedEvent>(OnBlockPowerChanged);

        Subs.BuiEvents<BlockGameArcadeComponent>(BlockGameUiKey.Key, subs =>
        {
            subs.Event<BoundUIClosedEvent>(OnAfterUiClose);
            subs.Event<BlockGameMessages.BlockGamePlayerActionMessage>(OnPlayerAction);
        });
    }

    public override void Update(float frameTime)
    {
        var query = EntityManager.EntityQueryEnumerator<BlockGameArcadeComponent>();
        while (query.MoveNext(out var _, out var blockGame))
        {
            blockGame.Game?.GameTick(frameTime);
        }
    }

    private void UpdatePlayerStatus(EntityUid uid, EntityUid actor, BlockGameArcadeComponent? blockGame = null)
    {
        if (!Resolve(uid, ref blockGame))
            return;

        _uiSystem.ServerSendUiMessage(uid, BlockGameUiKey.Key, new BlockGameMessages.BlockGameUserStatusMessage(blockGame.Player == actor), actor);
    }

    private void OnComponentInit(EntityUid uid, BlockGameArcadeComponent component, ComponentInit args)
    {
        component.Game = new(uid);
    }

    private void OnAfterUIOpen(EntityUid uid, BlockGameArcadeComponent component, AfterActivatableUIOpenEvent args)
    {
        if (component.Player == null)
            component.Player = args.Actor;
        else
            component.Spectators.Add(args.Actor);

        UpdatePlayerStatus(uid, args.Actor, component);
        component.Game?.UpdateNewPlayerUI(args.Actor);
    }

    private void OnAfterUiClose(EntityUid uid, BlockGameArcadeComponent component, BoundUIClosedEvent args)
    {
        if (component.Player != args.Actor)
        {
            component.Spectators.Remove(args.Actor);
            UpdatePlayerStatus(uid, args.Actor, blockGame: component);
            return;
        }

        var temp = component.Player;
        if (component.Spectators.Count > 0)
        {
            component.Player = component.Spectators[0];
            component.Spectators.Remove(component.Player.Value);
            UpdatePlayerStatus(uid, component.Player.Value, blockGame: component);
        }

        UpdatePlayerStatus(uid, temp.Value, blockGame: component);
    }

    private void OnBlockPowerChanged(EntityUid uid, BlockGameArcadeComponent component, ref PowerChangedEvent args)
    {
        if (args.Powered)
            return;

        _uiSystem.CloseUi(uid, BlockGameUiKey.Key);
        component.Player = null;
        component.Spectators.Clear();
    }

    private void OnPlayerAction(EntityUid uid, BlockGameArcadeComponent component, BlockGameMessages.BlockGamePlayerActionMessage msg)
    {
        if (component.Game == null)
            return;
        if (!BlockGameUiKey.Key.Equals(msg.UiKey))
            return;
        if (msg.Actor != component.Player)
            return;

        if (msg.PlayerAction == BlockGamePlayerAction.NewGame)
        {
            if (component.Game.Started == true)
                component.Game = new(uid);
            component.Game.StartGame();
            return;
        }

        if (TryComp<SpeakOnUIClosedComponent>(uid, out var speakComponent))
            _speakOnUIClosed.TrySetFlag((uid, speakComponent));

        component.Game.ProcessInput(msg.PlayerAction);
    }
}