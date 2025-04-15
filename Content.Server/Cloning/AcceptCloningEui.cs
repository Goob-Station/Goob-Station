// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Silver <silvertorch5@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 fishfish458 <fishfish458>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.EUI;
using Content.Shared.Cloning;
using Content.Shared.Eui;
using Content.Shared.Mind;

namespace Content.Server.Cloning
{
    public sealed class AcceptCloningEui : BaseEui
    {
        private readonly EntityUid _mindId;
        private readonly MindComponent _mind;
        private readonly CloningPodSystem _cloningPodSystem;

        public AcceptCloningEui(EntityUid mindId, MindComponent mind, CloningPodSystem cloningPodSys)
        {
            _mindId = mindId;
            _mind = mind;
            _cloningPodSystem = cloningPodSys;
        }

        public override void HandleMessage(EuiMessageBase msg)
        {
            base.HandleMessage(msg);

            if (msg is not AcceptCloningChoiceMessage choice ||
                choice.Button == AcceptCloningUiButton.Deny)
            {
                Close();
                return;
            }

            _cloningPodSystem.TransferMindToClone(_mindId, _mind);
            Close();
        }
    }
}