// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Access;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.CCVar;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.CrewManifest;
using Content.Shared.Roles;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using static Content.Shared.Access.Components.IdCardConsoleComponent;
#region Pirate: id card console fix
using System.Linq; 
using Robust.Client.Timing; 
using Content.Client.UserInterface; 
#endregion

namespace Content.Client.Access.UI
{
    public sealed class IdCardConsoleBoundUserInterface : BoundUserInterface
    {
        [Dependency] private readonly IClientGameTiming _gameTiming = default!; // Pirate: id card console fix
        private BuiPredictionState? _pred; // Pirate: id card console fix
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly IConfigurationManager _cfgManager = default!;
        private readonly SharedIdCardConsoleSystem _idCardConsoleSystem = default!;

        private IdCardConsoleWindow? _window;

        // CCVar.
        private int _maxNameLength;
        private int _maxIdJobLength;

        public IdCardConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
            IoCManager.InjectDependencies(this); // Pirate id card console fix
            _idCardConsoleSystem = EntMan.System<SharedIdCardConsoleSystem>();

            _maxNameLength = _cfgManager.GetCVar(CCVars.MaxNameLength);
            _maxIdJobLength = _cfgManager.GetCVar(CCVars.MaxIdJobLength);
        }

        protected override void Open()
        {
            base.Open();
            _pred = new BuiPredictionState(this, _gameTiming); // Pirate: id card console fix
            List<ProtoId<AccessLevelPrototype>> accessLevels;

            if (EntMan.TryGetComponent<IdCardConsoleComponent>(Owner, out var idCard))
            {
                accessLevels = idCard.AccessLevels;
            }
            else
            {
                accessLevels = new List<ProtoId<AccessLevelPrototype>>();
                _idCardConsoleSystem.Log.Error($"No IdCardConsole component found for {EntMan.ToPrettyString(Owner)}!");
            }

            _window = new IdCardConsoleWindow(this, _prototypeManager, accessLevels)
            {
                Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName
            };

            _window.CrewManifestButton.OnPressed += _ => SendMessage(new CrewManifestOpenUiMessage());
            _window.PrivilegedIdButton.OnPressed += _ => SendMessage(new ItemSlotButtonPressedEvent(PrivilegedIdCardSlotId));
            _window.TargetIdButton.OnPressed += _ => SendMessage(new ItemSlotButtonPressedEvent(TargetIdCardSlotId));

            _window.OnClose += Close;
            _window.OpenCentered();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;

            _window?.Dispose();
        }

        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);
            var castState = (IdCardConsoleBoundUserInterfaceState) state;
            castState = ApplyPredictedState(castState); // Pirate: id card console fix
            _window?.UpdateState(castState);
        }

        public void SubmitData(string newFullName, string newJobTitle, List<ProtoId<AccessLevelPrototype>> newAccessList, ProtoId<JobPrototype> newJobPrototype)
        {
            if (newFullName.Length > _maxNameLength)
                newFullName = newFullName[.._maxNameLength];

            if (newJobTitle.Length > _maxIdJobLength)
                newJobTitle = newJobTitle[.._maxIdJobLength];

            SendMessage(new WriteToTargetIdMessage(
                newFullName,
                newJobTitle,
                newAccessList,
                newJobPrototype));
        }

        #region Pirate: id card console fix
        private IdCardConsoleBoundUserInterfaceState ApplyPredictedState(IdCardConsoleBoundUserInterfaceState state)
        {
            if (_pred == null)
                return state;

            var predictedAccess = state.TargetIdAccessList?.ToHashSet() ?? new HashSet<ProtoId<AccessLevelPrototype>>();
            var changed = false;

            foreach (var replayMsg in _pred.MessagesToReplay())
            {
                if (replayMsg is not SetTargetIdAccessMessage accessMsg)
                    continue;

                changed = true;

                if (accessMsg.Enabled)
                    predictedAccess.Add(accessMsg.Access);
                else
                    predictedAccess.Remove(accessMsg.Access);
            }

            if (!changed)
                return state;

            return new IdCardConsoleBoundUserInterfaceState(
                state.IsPrivilegedIdPresent,
                state.IsPrivilegedIdAuthorized,
                state.IsTargetIdPresent,
                state.TargetIdFullName,
                state.TargetIdJobTitle,
                predictedAccess.ToList(),
                state.AllowedModifyAccessList?.ToList(),
                state.TargetIdJobPrototype,
                state.PrivilegedIdName,
                state.TargetIdName);
        }

        public void SetAccess(ProtoId<AccessLevelPrototype> access, bool enabled)
        {
            var message = new SetTargetIdAccessMessage(access, enabled);

            if (_pred != null)
                _pred.SendMessage(message);
            else
                SendPredictedMessage(message);
        }
        #endregion
    }
}
