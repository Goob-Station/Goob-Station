// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

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
