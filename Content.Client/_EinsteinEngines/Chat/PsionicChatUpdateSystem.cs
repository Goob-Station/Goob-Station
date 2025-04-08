using Content.Shared._EinsteinEngines.Abilities.Psionics;
using Content.Client.Chat.Managers;
using Robust.Client.Player;

namespace Content.Client.Chat
{
    public sealed class PsionicChatUpdateSystem : EntitySystem
    {
        [Dependency] private readonly IChatManager _chatManager = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<TelepathyComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<TelepathyComponent, ComponentRemove>(OnRemove);
        }

        public TelepathyComponent? Player => CompOrNull<TelepathyComponent>(_playerManager.LocalPlayer?.ControlledEntity);
        public bool IsPsionic => Player != null;

        private void OnInit(EntityUid uid, TelepathyComponent component, ComponentInit args)
        {
            _chatManager.UpdatePermissions();
        }

        private void OnRemove(EntityUid uid, TelepathyComponent component, ComponentRemove args)
        {
            _chatManager.UpdatePermissions();
        }
    }
}
