// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Rinary <72972221+Rinary1@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Chat.Managers;
using Content.Shared._Starlight.CollectiveMind;
using Robust.Client.Player;

namespace Content.Client.Chat
{
    public sealed class CollectiveMindSystem : EntitySystem
    {
        [Dependency] private readonly IChatManager _chatManager = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly CollectiveMindUpdateSystem _collectiveSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<CollectiveMindComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<CollectiveMindComponent, ComponentRemove>(OnRemove);
        }
        
        public bool IsCollectiveMind => CompOrNull<CollectiveMindComponent>(_playerManager.LocalPlayer?.ControlledEntity) != null;

        private void OnInit(EntityUid uid, CollectiveMindComponent component, ComponentInit args)
        {
            _chatManager.UpdatePermissions();
        }

        private void OnRemove(EntityUid uid, CollectiveMindComponent component, ComponentRemove args)
        {
            _chatManager.UpdatePermissions();
        }
    }
}