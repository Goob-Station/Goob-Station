// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kevin Zheng <kevinz5000@gmail.com>
// SPDX-FileCopyrightText: 2023 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 c4llv07e <38111072+c4llv07e@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Access;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Containers.ItemSlots;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;
using static Content.Shared.Access.Components.AccessOverriderComponent;

namespace Content.Client.Access.UI
{
    public sealed class AccessOverriderBoundUserInterface : BoundUserInterface
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        private readonly SharedAccessOverriderSystem _accessOverriderSystem = default!;

        private AccessOverriderWindow? _window;

        public AccessOverriderBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
            _accessOverriderSystem = EntMan.System<SharedAccessOverriderSystem>();
        }

        protected override void Open()
        {
            base.Open();

            _window = this.CreateWindow<AccessOverriderWindow>();
            RefreshAccess();
            _window.Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName;
            _window.OnSubmit += SubmitData;

            _window.PrivilegedIdButton.OnPressed += _ => SendMessage(new ItemSlotButtonPressedEvent(PrivilegedIdCardSlotId));
        }

        public override void OnProtoReload(PrototypesReloadedEventArgs args)
        {
            base.OnProtoReload(args);
            if (!args.WasModified<AccessLevelPrototype>())
                return;

            RefreshAccess();

            if (State != null)
                _window?.UpdateState(_prototypeManager, (AccessOverriderBoundUserInterfaceState) State);
        }

        private void RefreshAccess()
        {
            List<ProtoId<AccessLevelPrototype>> accessLevels;

            if (EntMan.TryGetComponent<AccessOverriderComponent>(Owner, out var accessOverrider))
            {
                accessLevels = accessOverrider.AccessLevels;
                accessLevels.Sort();
            }
            else
            {
                accessLevels = new List<ProtoId<AccessLevelPrototype>>();
                _accessOverriderSystem.Log.Error($"No AccessOverrider component found for {EntMan.ToPrettyString(Owner)}!");
            }

            _window?.SetAccessLevels(_prototypeManager, accessLevels);
        }

        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);
            var castState = (AccessOverriderBoundUserInterfaceState) state;
            _window?.UpdateState(_prototypeManager, castState);
        }

        public void SubmitData(List<ProtoId<AccessLevelPrototype>> newAccessList)
        {
            SendMessage(new WriteToTargetAccessReaderIdMessage(newAccessList));
        }
    }
}