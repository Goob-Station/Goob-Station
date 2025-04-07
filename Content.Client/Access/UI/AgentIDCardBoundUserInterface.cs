// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Watermelon914 <37270891+Watermelon914@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 PrPleGoo <PrPleGoo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Vordenburg <114301317+Vordenburg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Skubman <ba.fallaria@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ty Ashley <42426760+TyAshley@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Access.Systems;
using Content.Shared.StatusIcon;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client.Access.UI
{
    /// <summary>
    /// Initializes a <see cref="AgentIDCardWindow"/> and updates it when new server messages are received.
    /// </summary>
    public sealed class AgentIDCardBoundUserInterface : BoundUserInterface
    {
        private AgentIDCardWindow? _window;

        public AgentIDCardBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
        }

        protected override void Open()
        {
            base.Open();

            _window = this.CreateWindow<AgentIDCardWindow>();

            _window.OnNameChanged += OnNameChanged;
            _window.OnJobChanged += OnJobChanged;
            _window.OnJobIconChanged += OnJobIconChanged;
            _window.OnNumberChanged += OnNumberChanged; // DeltaV
        }

        // DeltaV - Add number change handler
        private void OnNumberChanged(uint newNumber)
        {
            SendMessage(new AgentIDCardNumberChangedMessage(newNumber));
        }

        private void OnNameChanged(string newName)
        {
            SendMessage(new AgentIDCardNameChangedMessage(newName));
        }

        private void OnJobChanged(string newJob)
        {
            SendMessage(new AgentIDCardJobChangedMessage(newJob));
        }

        public void OnJobIconChanged(ProtoId<JobIconPrototype> newJobIconId)
        {
            SendMessage(new AgentIDCardJobIconChangedMessage(newJobIconId));
        }

        /// <summary>
        /// Update the UI state based on server-sent info
        /// </summary>
        /// <param name="state"></param>
        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);
            if (_window == null || state is not AgentIDCardBoundUserInterfaceState cast)
                return;

            _window.SetCurrentName(cast.CurrentName);
            _window.SetCurrentJob(cast.CurrentJob);
            _window.SetAllowedIcons(cast.CurrentJobIconId);
            _window.SetCurrentNumber(cast.CurrentNumber); // DeltaV
        }
    }
}