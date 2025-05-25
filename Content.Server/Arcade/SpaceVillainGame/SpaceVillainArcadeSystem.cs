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

using Content.Server.Power.Components;
using Content.Shared.UserInterface;
using Content.Server.Advertise.EntitySystems;
using Content.Shared.Advertise.Components;
using Content.Shared.Arcade;
using Content.Shared.Power;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;

namespace Content.Server.Arcade.SpaceVillain;

public sealed partial class SpaceVillainArcadeSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly SpeakOnUIClosedSystem _speakOnUIClosed = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpaceVillainArcadeComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<SpaceVillainArcadeComponent, AfterActivatableUIOpenEvent>(OnAfterUIOpenSV);
        SubscribeLocalEvent<SpaceVillainArcadeComponent, SharedSpaceVillainArcadeComponent.SpaceVillainArcadePlayerActionMessage>(OnSVPlayerAction);
        SubscribeLocalEvent<SpaceVillainArcadeComponent, PowerChangedEvent>(OnSVillainPower);
    }

    /// <summary>
    /// Called when the user wins the game.
    /// Dispenses a prize if the arcade machine has any left.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="arcade"></param>
    /// <param name="xform"></param>
    public void ProcessWin(EntityUid uid, SpaceVillainArcadeComponent? arcade = null, TransformComponent? xform = null)
    {
        if (!Resolve(uid, ref arcade, ref xform))
            return;
        if (arcade.RewardAmount <= 0)
            return;

        EntityManager.SpawnEntity(_random.Pick(arcade.PossibleRewards), xform.Coordinates);
        arcade.RewardAmount--;
    }

    /// <summary>
    /// Picks a fight-verb from the list of possible Verbs.
    /// </summary>
    /// <returns>A fight-verb.</returns>
    public string GenerateFightVerb(SpaceVillainArcadeComponent arcade)
    {
        return _random.Pick(arcade.PossibleFightVerbs);
    }

    /// <summary>
    /// Generates an enemy-name comprised of a first- and last-name.
    /// </summary>
    /// <returns>An enemy-name.</returns>
    public string GenerateEnemyName(SpaceVillainArcadeComponent arcade)
    {
        return $"{_random.Pick(arcade.PossibleFirstEnemyNames)} {_random.Pick(arcade.PossibleLastEnemyNames)}";
    }

    private void OnComponentInit(EntityUid uid, SpaceVillainArcadeComponent component, ComponentInit args)
    {
        // Random amount of prizes
        component.RewardAmount = new Random().Next(component.RewardMinAmount, component.RewardMaxAmount + 1);
    }

    private void OnSVPlayerAction(EntityUid uid, SpaceVillainArcadeComponent component, SharedSpaceVillainArcadeComponent.SpaceVillainArcadePlayerActionMessage msg)
    {
        if (component.Game == null)
            return;
        if (!TryComp<ApcPowerReceiverComponent>(uid, out var power) || !power.Powered)
            return;

        switch (msg.PlayerAction)
        {
            case SharedSpaceVillainArcadeComponent.PlayerAction.Attack:
            case SharedSpaceVillainArcadeComponent.PlayerAction.Heal:
            case SharedSpaceVillainArcadeComponent.PlayerAction.Recharge:
                component.Game.ExecutePlayerAction(uid, msg.PlayerAction, component);
                // Any sort of gameplay action counts
                if (TryComp<SpeakOnUIClosedComponent>(uid, out var speakComponent))
                    _speakOnUIClosed.TrySetFlag((uid, speakComponent));
                break;
            case SharedSpaceVillainArcadeComponent.PlayerAction.NewGame:
                _audioSystem.PlayPvs(component.NewGameSound, uid, AudioParams.Default.WithVolume(-4f));

                component.Game = new SpaceVillainGame(uid, component, this);
                _uiSystem.ServerSendUiMessage(uid, SharedSpaceVillainArcadeComponent.SpaceVillainArcadeUiKey.Key, component.Game.GenerateMetaDataMessage());
                break;
            case SharedSpaceVillainArcadeComponent.PlayerAction.RequestData:
                _uiSystem.ServerSendUiMessage(uid, SharedSpaceVillainArcadeComponent.SpaceVillainArcadeUiKey.Key, component.Game.GenerateMetaDataMessage());
                break;
        }
    }

    private void OnAfterUIOpenSV(EntityUid uid, SpaceVillainArcadeComponent component, AfterActivatableUIOpenEvent args)
    {
        component.Game ??= new(uid, component, this);
    }

    private void OnSVillainPower(EntityUid uid, SpaceVillainArcadeComponent component, ref PowerChangedEvent args)
    {
        if (TryComp<ApcPowerReceiverComponent>(uid, out var power) && power.Powered)
            return;

        _uiSystem.CloseUi(uid, SharedSpaceVillainArcadeComponent.SpaceVillainArcadeUiKey.Key);
    }
}