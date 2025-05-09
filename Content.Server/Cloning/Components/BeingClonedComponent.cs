// SPDX-FileCopyrightText: 2021 Silver <silvertorch5@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Mind;

namespace Content.Server.Cloning.Components
{
    [RegisterComponent]
    public sealed partial class BeingClonedComponent : Component
    {
        [ViewVariables]
        public MindComponent? Mind = default;

        [ViewVariables]
        public EntityUid Parent;
    }
}