// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 mirrorcult <notzombiedude@gmail.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared.Arcade;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;

namespace Content.Client.Arcade
{
    public sealed class SpaceVillainArcadeMenu : DefaultWindow
    {
        private readonly Label _enemyNameLabel;
        private readonly Label _playerInfoLabel;
        private readonly Label _enemyInfoLabel;
        private readonly Label _playerActionLabel;
        private readonly Label _enemyActionLabel;

        private readonly Button[] _gameButtons = new Button[3]; //used to disable/enable all game buttons

        public event Action<SharedSpaceVillainArcadeComponent.PlayerAction>? OnPlayerAction;

        public SpaceVillainArcadeMenu()
        {
            MinSize = SetSize = new Vector2(300, 225);
            Title = Loc.GetString("spacevillain-menu-title");

            var grid = new GridContainer { Columns = 1 };

            var infoGrid = new GridContainer { Columns = 3 };
            infoGrid.AddChild(new Label { Text = Loc.GetString("spacevillain-menu-label-player"), Align = Label.AlignMode.Center });
            infoGrid.AddChild(new Label { Text = "|", Align = Label.AlignMode.Center });
            _enemyNameLabel = new Label { Align = Label.AlignMode.Center };
            infoGrid.AddChild(_enemyNameLabel);

            _playerInfoLabel = new Label { Align = Label.AlignMode.Center };
            infoGrid.AddChild(_playerInfoLabel);
            infoGrid.AddChild(new Label { Text = "|", Align = Label.AlignMode.Center });
            _enemyInfoLabel = new Label { Align = Label.AlignMode.Center };
            infoGrid.AddChild(_enemyInfoLabel);
            var centerContainer = new CenterContainer();
            centerContainer.AddChild(infoGrid);
            grid.AddChild(centerContainer);

            _playerActionLabel = new Label { Align = Label.AlignMode.Center };
            grid.AddChild(_playerActionLabel);

            _enemyActionLabel = new Label { Align = Label.AlignMode.Center };
            grid.AddChild(_enemyActionLabel);

            var buttonGrid = new GridContainer { Columns = 3 };
            _gameButtons[0] = new Button()
            {
                Text = Loc.GetString("spacevillain-menu-button-attack")
            };

            _gameButtons[0].OnPressed +=
                _ => OnPlayerAction?.Invoke(SharedSpaceVillainArcadeComponent.PlayerAction.Attack);
            buttonGrid.AddChild(_gameButtons[0]);

            _gameButtons[1] = new Button()
            {
                Text = Loc.GetString("spacevillain-menu-button-heal")
            };

            _gameButtons[1].OnPressed +=
                _ => OnPlayerAction?.Invoke(SharedSpaceVillainArcadeComponent.PlayerAction.Heal);
            buttonGrid.AddChild(_gameButtons[1]);

            _gameButtons[2] = new Button()
            {
                Text = Loc.GetString("spacevillain-menu-button-recharge")
            };

            _gameButtons[2].OnPressed +=
                _ => OnPlayerAction?.Invoke(SharedSpaceVillainArcadeComponent.PlayerAction.Recharge);
            buttonGrid.AddChild(_gameButtons[2]);

            centerContainer = new CenterContainer();
            centerContainer.AddChild(buttonGrid);
            grid.AddChild(centerContainer);

            var newGame = new Button()
            {
                Text = Loc.GetString("spacevillain-menu-button-new-game")
            };

            newGame.OnPressed += _ => OnPlayerAction?.Invoke(SharedSpaceVillainArcadeComponent.PlayerAction.NewGame);
            grid.AddChild(newGame);

            Contents.AddChild(grid);
        }

        private void UpdateMetadata(SharedSpaceVillainArcadeComponent.SpaceVillainArcadeMetaDataUpdateMessage message)
        {
            Title = message.GameTitle;
            _enemyNameLabel.Text = message.EnemyName;

            foreach (var gameButton in _gameButtons)
            {
                gameButton.Disabled = message.ButtonsDisabled;
            }
        }

        public void UpdateInfo(SharedSpaceVillainArcadeComponent.SpaceVillainArcadeDataUpdateMessage message)
        {
            if (message is SharedSpaceVillainArcadeComponent.SpaceVillainArcadeMetaDataUpdateMessage metaMessage)
                UpdateMetadata(metaMessage);

            _playerInfoLabel.Text = $"HP: {message.PlayerHP} MP: {message.PlayerMP}";
            _enemyInfoLabel.Text = $"HP: {message.EnemyHP} MP: {message.EnemyMP}";
            _playerActionLabel.Text = message.PlayerActionMessage;
            _enemyActionLabel.Text = message.EnemyActionMessage;
        }
    }
}