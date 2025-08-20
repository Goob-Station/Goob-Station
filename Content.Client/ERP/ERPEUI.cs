using Content.Client.Eui;
using Content.Shared.Eui;
using Content.Shared.ERP;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Content.Shared.ERP;
using System.Linq;
using Content.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Random;
using Content.Client.Chat.Managers;
using Robust.Shared.Audio.Systems;
using Robust.Client.Player;
using Robust.Shared.Timing;
using Content.Shared.IdentityManagement;
namespace Content.Client.ERP
{
    [UsedImplicitly]
    public sealed class ERPEUI : BaseEui
    {
        private readonly ERPUI _window;
        public IEntityManager _entManager;

        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly IChatManager _chat = default!;
        [Dependency] private readonly IPlayerManager _player = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        private readonly SharedAudioSystem _audio; 
        public ERPEUI()
        {
            _entManager = IoCManager.Resolve<IEntityManager>();
            _window = new ERPUI(this);
            _window.OnClose += OnClosed;

            _audio = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<SharedAudioSystem>();
        }

        public override void HandleMessage(EuiMessageBase msg)
        {
            base.HandleMessage(msg);

            switch (msg)
            {
                case ResponseLoveMessage req:
                    _window.LoveBar.Value = req.Percent;
                    break;
                case ResponseInteractionState req:
                    _window.UserHasClothing = req.UserHasClothing;
                    _window.TargetHasClothing = req.TargetHasClothing;
                    _window.UserSex = req.UserSex;
                    _window.TargetSex = req.TargetSex;
                    _window.Erp = req.ErpAllowed;
                    _window.Populate();
                    break;
            }
        }

        public void RequestLove()
        {
            if (!_player.LocalEntity.HasValue) return;
            if (!_window.TargetEntityId.HasValue) return;
            SendMessage(new AddLoveMessage(_entManager.GetNetEntity(_player.LocalEntity.Value), _window.TargetEntityId.Value, 0));
        }

        public void RequestState()
        {
            if (!_window.TargetEntityId.HasValue) return;
            if (!_player.LocalEntity.HasValue) return;
            SendMessage(new RequestInteractionState(_entManager.GetNetEntity(_player.LocalEntity.Value), _window.TargetEntityId.Value));
        }

        private void OnClosed()
        {
            SendMessage(new CloseEuiMessage());
        }

        public override void Opened()
        {
            _window.OpenCentered();
        }

        public override void Closed()
        {
            base.Closed();
            _window.Close();
        }

        public override void HandleState(EuiStateBase state)
        {
            base.HandleState(state);
            var euiState = (SetInteractionEuiState) state;
            _window.TargetEntityId = euiState.TargetNetEntity;
            _window.UserSex = euiState.UserSex;
            _window.TargetSex = euiState.TargetSex;
            _window.UserHasClothing = euiState.UserHasClothing;
            _window.TargetHasClothing = euiState.TargetHasClothing;
            _window.Erp = euiState.ErpAllowed;
            _window.Populate();

        }

        public void OnItemSelect(ItemList.ItemListSelectedEventArgs args)
        {
            if (_gameTiming.CurTime >= _window.TimeUntilAllow)
            {
                if (!_player.LocalEntity.HasValue) return;
                var item = args.ItemList[args.ItemIndex];
                if (item.Metadata == null) return;
                ERPPrototype interaction = (ERPPrototype) item.Metadata;
                if (interaction.Emotes.Count > 0)
                {
                    if (_window.TargetEntityId == null) return;
                    string emote = _random.Pick(interaction.Emotes);
                    emote = emote.Replace("%user", Identity.Name(_player.LocalEntity.Value, _entManager));
                    emote = emote.Replace("%target", Identity.Name(_entManager.GetEntity(_window.TargetEntityId.Value), _entManager));
                    _chat.SendMessage(emote, Shared.Chat.ChatSelectChannel.Emotes);
                }
                if (interaction.Sounds.Count > 0)
                {

                    _audio.PlayPvs(_random.Pick(interaction.Sounds), _player.LocalEntity.Value);
                }
                if (!_window.TargetEntityId.HasValue) return;
                SendMessage(new AddLoveMessage(_entManager.GetNetEntity(_player.LocalEntity.Value), _window.TargetEntityId.Value, interaction.LovePercent));
                _window.TimeUntilAllow = _gameTiming.CurTime + TimeSpan.FromSeconds(2);
            }
        }
    }
}
